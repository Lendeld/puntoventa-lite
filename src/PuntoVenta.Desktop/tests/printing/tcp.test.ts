import { describe, it, expect } from "vitest";
import { parseTcpEndpoint } from "../../src/printing/transports/tcp";

describe("parseTcpEndpoint — port de TcpRawPrinter.TryParseEndpoint", () => {
    // Casos válidos
    it("parsea tcp://host:puerto", () => {
        const r = parseTcpEndpoint("tcp://192.168.1.100:9100");
        expect(r).toEqual({ host: "192.168.1.100", port: 9100 });
    });

    it("parsea socket://host:puerto", () => {
        const r = parseTcpEndpoint("socket://192.168.1.100:9100");
        expect(r).toEqual({ host: "192.168.1.100", port: 9100 });
    });

    it("parsea IP sin prefijo con puerto", () => {
        const r = parseTcpEndpoint("192.168.1.100:9100");
        expect(r).toEqual({ host: "192.168.1.100", port: 9100 });
    });

    it("parsea IP sin puerto → default 9100", () => {
        const r = parseTcpEndpoint("192.168.1.100");
        expect(r).toEqual({ host: "192.168.1.100", port: 9100 });
    });

    it("parsea hostname con punto y puerto", () => {
        const r = parseTcpEndpoint("printer.local:9100");
        expect(r).toEqual({ host: "printer.local", port: 9100 });
    });

    it("parsea hostname con punto sin puerto → default 9100", () => {
        const r = parseTcpEndpoint("mi-impresora.local");
        expect(r).toEqual({ host: "mi-impresora.local", port: 9100 });
    });

    it("ignora query string (contimeout, waiteof, etc.)", () => {
        const r = parseTcpEndpoint("socket://192.168.1.100:9100?contimeout=5000&waiteof=false");
        expect(r).toEqual({ host: "192.168.1.100", port: 9100 });
    });

    it("ignora slash final", () => {
        const r = parseTcpEndpoint("tcp://192.168.1.100:9100/");
        expect(r).toEqual({ host: "192.168.1.100", port: 9100 });
    });

    it("parsea puerto no-standard", () => {
        const r = parseTcpEndpoint("tcp://192.168.1.200:515");
        expect(r).toEqual({ host: "192.168.1.200", port: 515 });
    });

    it("parsea socket:// uppercase sin puerto → default 9100", () => {
        const r = parseTcpEndpoint("SOCKET://192.168.1.50");
        expect(r).toEqual({ host: "192.168.1.50", port: 9100 });
    });

    // Casos inválidos
    it("retorna null para string vacío", () => {
        expect(parseTcpEndpoint("")).toBeNull();
    });

    it("retorna null para nombre de cola CUPS sin punto", () => {
        expect(parseTcpEndpoint("EPSON-TM20III")).toBeNull();
    });

    it("retorna null para nombre de cola CUPS con espacios", () => {
        expect(parseTcpEndpoint("mi impresora")).toBeNull();
    });

    it("retorna null para serial://", () => {
        expect(parseTcpEndpoint("serial:///dev/ttyUSB0")).toBeNull();
    });

    it("retorna null para host vacío después de protocolo", () => {
        expect(parseTcpEndpoint("tcp://")).toBeNull();
    });

    it("retorna null para hostname con espacios (inyección)", () => {
        expect(parseTcpEndpoint("host with spaces:9100")).toBeNull();
    });
});
