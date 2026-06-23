// Se ejecuta apenas carga cualquier página
document.addEventListener('DOMContentLoaded', () => {
    actualizarContadorCarrito();

    // Si estamos en la página del carrito, lo dibujamos
    if (window.location.pathname.toLowerCase().includes('/tienda/carrito')) {
        renderizarCarrito();
    }
});

function obtenerCarrito() {
    return JSON.parse(localStorage.getItem('bastion_carrito')) || [];
}

function guardarCarrito(carrito) {
    localStorage.setItem('bastion_carrito', JSON.stringify(carrito));
    actualizarContadorCarrito();
}

function actualizarContadorCarrito() {
    const carrito = obtenerCarrito();
    const totalItems = carrito.reduce((sum, item) => sum + parseInt(item.cantidad), 0);
    const contador = document.getElementById('contador-carrito');
    if (contador) {
        contador.innerText = totalItems;
    }
}

function agregarAlCarrito(id, nombre, precio, esServicio) {
    const inputCantidad = document.getElementById('cantidad-item');
    const cantidad = inputCantidad ? parseInt(inputCantidad.value) : 1;

    let carrito = obtenerCarrito();
    let itemExistente = carrito.find(x => x.productoId === id);

    if (itemExistente) {
        itemExistente.cantidad += cantidad;
    } else {
        carrito.push({ productoId: id, nombre: nombre, precioUnitario: parseFloat(precio), cantidad: cantidad, esServicio: esServicio });
    }

    guardarCarrito(carrito);
    alert('¡Añadido al carrito con éxito!');
}

function eliminarDelCarrito(id) {
    let carrito = obtenerCarrito();
    carrito = carrito.filter(x => x.productoId !== id);
    guardarCarrito(carrito);
    renderizarCarrito();
}

function vaciarCarrito() {
    localStorage.removeItem('bastion_carrito');
    actualizarContadorCarrito();
    renderizarCarrito();
}

function renderizarCarrito() {
    const contenedorItems = document.getElementById('contenedor-carrito-items');
    const contenedorTotales = document.getElementById('contenedor-totales');
    const spanTotal = document.getElementById('total-carrito');
    const carrito = obtenerCarrito();

    if (carrito.length === 0) {
        contenedorItems.innerHTML = '<div class="p-10 text-center text-slate-500 font-medium">Tu carrito está vacío.</div>';
        contenedorTotales.classList.add('hidden');
        return;
    }

    contenedorTotales.classList.remove('hidden');
    let html = '';
    let totalCalculado = 0;

    carrito.forEach(item => {
        const subtotal = item.precioUnitario * item.cantidad;
        totalCalculado += subtotal;

        html += `
            <div class="flex justify-between items-center p-6 border-b border-slate-100 hover:bg-slate-50">
                <div class="flex-1">
                    <h3 class="font-bold text-slate-900 text-lg">${item.nombre}</h3>
                    <p class="text-sm text-slate-500">Precio Unitario: $${item.precioUnitario.toLocaleString('es-AR')}</p>
                </div>
                <div class="font-bold text-slate-700 w-24 text-center">x ${item.cantidad}</div>
                <div class="font-black text-blue-800 w-32 text-right text-lg">$${subtotal.toLocaleString('es-AR')}</div>
                <button onclick="eliminarDelCarrito(${item.productoId})" class="ml-6 text-rose-500 hover:text-rose-700 transition-colors">
                    <i class="fas fa-trash"></i>
                </button>
            </div>
        `;
    });

    contenedorItems.innerHTML = html;
    spanTotal.innerText = '$' + totalCalculado.toLocaleString('es-AR');
}

async function procesarCheckout() {
    const carrito = obtenerCarrito();
    if (carrito.length === 0) return;

    let totalCalculado = carrito.reduce((sum, item) => sum + (item.precioUnitario * item.cantidad), 0);

    const payload = {
        ClienteId: "usuario-demo", // Hardcodeado hasta implementar Login
        Items: carrito,
        TotalCalculado: totalCalculado
    };

    try {
        const response = await fetch('/Tienda/ProcesarCheckout', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (response.ok) {
            alert('¡Compra realizada con éxito! (Simulación Backend conectada)');
            vaciarCarrito();
            window.location.href = '/Tienda/Index';
        } else {
            alert('Error procesando el pedido en el servidor.');
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Fallo de comunicación con el servidor.');
    }
}