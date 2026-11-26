"use strict";
function confirmarEliminacion(event) {
  const form = event.target;
  if (form.matches("form[data-confirm]") && !window.confirm(form.dataset.confirm)) event.preventDefault();
}
function alternarImporte(button) {
  const amount = document.querySelector("[data-total]");
  if (!amount) return;
  amount.dataset.original ??= amount.textContent;
  const hidden = button.getAttribute("aria-pressed") !== "true";
  amount.textContent = hidden ? "••••••" : amount.dataset.original;
  button.setAttribute("aria-pressed", String(hidden));
  button.textContent = hidden ? "Mostrar importe" : "Ocultar importe";
}
document.addEventListener("submit", confirmarEliminacion);
document.addEventListener("click", (event) => {
  const button = event.target.closest("button");
  if (!button) return;
  if (button.matches("[data-visibility]")) alternarImporte(button);
  if (button.matches("[data-print]")) window.print();
});
