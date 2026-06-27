const CLAVE_CARRITO = 'bastion_carrito';

document.addEventListener('DOMContentLoaded', () => {
    actualizarContadorCarrito();
    if (window.location.pathname.toLowerCase().includes('/tienda/carrito')) {
        renderizarCarrito();
    }
});

function obtenerCarrito() {
    return JSON.parse(localStorage.getItem(CLAVE_CARRITO)) || [];
}

function guardarCarrito(carrito) {
    localStorage.setItem(CLAVE_CARRITO, JSON.stringify(carrito));
    actualizarContadorCarrito();
}

function actualizarContadorCarrito() {
    const carrito = obtenerCarrito();
    const totalItems = carrito.reduce((sum, item) => sum + parseInt(item.cantidad), 0);
    const contador = document.getElementById('contador-carrito');
    if (contador) contador.innerText = totalItems;
}

function agregarAlCarrito(id, nombre, precio, esServicio) {
    const inputCantidad = document.getElementById('cantidad-item');
    const cantidad = inputCantidad ? parseInt(inputCantidad.value) : 1;

    let carrito = obtenerCarrito();
    // Validamos por id o productoId para evitar duplicados
    let itemExistente = carrito.find(x => x.id === id || x.productoId === id);

    if (itemExistente) {
        itemExistente.cantidad += cantidad;
    } else {
        carrito.push({ id: id, nombre: nombre, precio: parseFloat(precio), cantidad: cantidad, esServicio: esServicio });
    }

    guardarCarrito(carrito);
    alert('¡Añadido al carrito con éxito!');
}

function actualizarCantidad(id, cantidadCambio) {
    let carrito = obtenerCarrito();
    const index = carrito.findIndex(p => p.id === id || p.productoId === id);

    if (index !== -1) {
        carrito[index].cantidad += cantidadCambio;
        if (carrito[index].cantidad <= 0) carrito.splice(index, 1);

        guardarCarrito(carrito);
        renderizarCarrito();
    }
}

function eliminarDelCarrito(id) {
    let carrito = obtenerCarrito();
    carrito = carrito.filter(x => x.id !== id && x.productoId !== id);
    guardarCarrito(carrito);
    renderizarCarrito();
}

function vaciarCarrito() {
    if (confirm("¿Estás seguro de vaciar el carrito?")) {
        localStorage.removeItem(CLAVE_CARRITO);
        actualizarContadorCarrito();
        renderizarCarrito();
    }
}

function renderizarCarrito() {
    const contenedorItems = document.getElementById('contenedor-carrito-items');
    const contenedorTotales = document.getElementById('contenedor-totales');
    const spanTotal = document.getElementById('total-carrito');

    if (!contenedorItems) return;

    const carrito = obtenerCarrito();

    if (carrito.length === 0) {
        contenedorItems.innerHTML = '<div class="p-12 text-center text-slate-500 text-lg font-medium">Tu carrito está vacío. ¡Ve a la tienda a buscar algo genial! 🛒</div>';
        contenedorTotales.classList.add('hidden');
        return;
    }

    contenedorTotales.classList.remove('hidden');
    let html = '';
    let sumaTotal = 0;

    carrito.forEach(item => {
        const subtotal = item.precio * item.cantidad;
        sumaTotal += subtotal;

        html += `
            <div class="flex items-center justify-between p-6 border-b border-slate-100 last:border-b-0 hover:bg-slate-50 transition-colors">
                <div class="flex-1">
                    <h3 class="text-lg font-bold text-slate-800">${item.nombre}</h3>
                    <p class="text-sm font-medium ${item.esServicio ? 'text-purple-600' : 'text-slate-500'}">
                        ${item.esServicio ? '🔧 Servicio IT' : '📦 Hardware'}
                    </p>
                    <p class="text-slate-600 mt-1">Precio: $${item.precio.toFixed(2)} c/u</p>
                </div>

                <div class="flex items-center gap-6">
                    <div class="flex items-center bg-white border border-slate-200 rounded-lg shadow-sm">
                        <button onclick="actualizarCantidad(${item.id || item.productoId}, -1)" class="px-4 py-2 text-slate-600 hover:bg-slate-100 rounded-l-lg font-bold transition-colors">-</button>
                        <span class="px-4 font-bold text-slate-800">${item.cantidad}</span>
                        <button onclick="actualizarCantidad(${item.id || item.productoId}, 1)" class="px-4 py-2 text-slate-600 hover:bg-slate-100 rounded-r-lg font-bold transition-colors">+</button>
                    </div>

                    <div class="text-right w-24">
                        <span class="block font-black text-slate-800 text-lg">$${subtotal.toFixed(2)}</span>
                    </div>

                    <button onclick="eliminarDelCarrito(${item.id || item.productoId})" class="text-red-400 hover:text-red-600 hover:bg-red-50 p-2 rounded-lg transition-colors" title="Eliminar del carrito">
                        <i class="fas fa-trash text-xl"></i>
                    </button>
                </div>
            </div>
        `;
    });

    contenedorItems.innerHTML = html;
    spanTotal.innerText = '$' + sumaTotal.toFixed(2);
}

async function procesarCheckout() {
    const carrito = obtenerCarrito();
    if (carrito.length === 0) return;

    const btn = document.getElementById('btn-checkout');
    if (btn) { btn.disabled = true; btn.innerText = 'Procesando pago...'; btn.classList.add('opacity-75'); }

    const pedidoDTO = {
        ClienteId: "",
        TotalCalculado: 0,
        Items: carrito.map(item => ({
            ProductoId: item.id || item.productoId,
            Cantidad: item.cantidad,
            PrecioUnitario: item.precio,
            EsServicio: item.esServicio
        }))
    };

    try {
        const response = await fetch('/Tienda/ProcesarCheckout', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(pedidoDTO)
        });

        const data = await response.json();

        if (response.ok) {
            alert("¡Éxito! " + data.mensaje);
            localStorage.removeItem(CLAVE_CARRITO);
            window.location.href = '/Tienda/Index';
        } else {
            alert("Ups: " + data.mensaje);
            if (btn) { btn.disabled = false; btn.innerText = 'Finalizar Compra'; btn.classList.remove('opacity-75'); }
        }
    } catch (error) {
        console.error('Error conectando al servidor:', error);
        alert("Hubo un error de conexión con el servidor.");
        if (btn) { btn.disabled = false; btn.innerText = 'Finalizar Compra'; btn.classList.remove('opacity-75'); }
    }
}