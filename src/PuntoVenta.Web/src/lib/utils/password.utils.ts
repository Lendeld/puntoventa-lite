// Genera una contraseña temporal que cumple el formato exigido por el schema
// (8+ chars, mayúscula, minúscula, dígito y símbolo). Se excluyen caracteres
// ambiguos (O/0, l/1/I) para que sea fácil de dictar/copiar.
const MAYUSCULAS = "ABCDEFGHJKLMNPQRSTUVWXYZ";
const MINUSCULAS = "abcdefghijkmnpqrstuvwxyz";
const DIGITOS = "23456789";
const SIMBOLOS = "$&+=?@#%!-";
const TODOS = MAYUSCULAS + MINUSCULAS + DIGITOS + SIMBOLOS;

const LONGITUD = 12;

function elegir(charset: string): string {
    const buffer = new Uint32Array(1);
    crypto.getRandomValues(buffer);
    return charset[buffer[0] % charset.length];
}

export function generarPasswordTemporal(): string {
    // Garantiza al menos uno de cada clase, luego rellena y mezcla.
    const chars = [
        elegir(MAYUSCULAS),
        elegir(MINUSCULAS),
        elegir(DIGITOS),
        elegir(SIMBOLOS),
    ];
    while (chars.length < LONGITUD) {
        chars.push(elegir(TODOS));
    }
    // Fisher–Yates con aleatoriedad criptográfica.
    for (let i = chars.length - 1; i > 0; i--) {
        const buffer = new Uint32Array(1);
        crypto.getRandomValues(buffer);
        const j = buffer[0] % (i + 1);
        [chars[i], chars[j]] = [chars[j], chars[i]];
    }
    return chars.join("");
}
