export function money(n: number, c = "$") {
  return c + n.toFixed(2).replace(/\d(?=(\d{3})+\.)/g, "$&,");
}
