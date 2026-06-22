import { describe, it, expect } from "vitest";
import { parseTcpEndpoint } from "../../src/printing/transports/tcp";
import { parseSerialEndpoint } from "../../src/printing/transports/serial";

/**
 * Router de transports — verifica que el router identifica correctamente
 * el tipo de transport para cada designación de impresora.
 */
describe("Router de transports — detección por designación", () => {
    describe("Designaciones TCP", () => {
        const tcpCases = [
            "tcp://192.168.1.100:9100",
            "socket://192.168.1.100:9100",
            "192.168.1.100:9100",
            "192.168.1.100",
            "printer.local:9100",
        ];

        for (const name of tcpCases) {
            it(`"${name}" → TCP (no serial, no sistema)`, () => {
                expect(parseSerialEndpoint(name)).toBeNull();
                expect(parseTcpEndpoint(name)).not.toBeNull();
            });
        }
    });

    describe("Designaciones serial", () => {
        const serialCases = [
            "serial:///dev/ttyUSB0",
            "serial:///dev/ttyUSB0?baud=115200",
            "serial://COM3",
            "serial://COM3?baud=9600",
        ];

        for (const name of serialCases) {
            it(`"${name}" → serial (no TCP)`, () => {
                expect(parseSerialEndpoint(name)).not.toBeNull();
                expect(parseTcpEndpoint(name)).toBeNull();
            });
        }
    });

    describe("Designaciones de cola del sistema", () => {
        const sistemaCases = [
            "EPSON-TM20III",
            "HP-LaserJet",
            "Brother-HL-L2350DW",
        ];

        for (const name of sistemaCases) {
            it(`"${name}" → sistema (ni TCP ni serial)`, () => {
                expect(parseSerialEndpoint(name)).toBeNull();
                expect(parseTcpEndpoint(name)).toBeNull();
            });
        }
    });
});

describe("parseSerialEndpoint", () => {
    it("parsea path y baud rate", () => {
        const r = parseSerialEndpoint("serial:///dev/ttyUSB0?baud=115200");
        expect(r).toEqual({ path: "/dev/ttyUSB0", baud: 115200 });
    });

    it("default baud 9600", () => {
        const r = parseSerialEndpoint("serial:///dev/ttyUSB0");
        expect(r).toEqual({ path: "/dev/ttyUSB0", baud: 9600 });
    });

    it("parsea COM port Windows", () => {
        const r = parseSerialEndpoint("serial://COM3?baud=9600");
        expect(r).toEqual({ path: "COM3", baud: 9600 });
    });

    it("retorna null para designaciones no seriales", () => {
        expect(parseSerialEndpoint("192.168.1.100")).toBeNull();
        expect(parseSerialEndpoint("EPSON")).toBeNull();
        expect(parseSerialEndpoint("tcp://192.168.1.100")).toBeNull();
    });

    it("retorna null para serial sin path", () => {
        expect(parseSerialEndpoint("serial://")).toBeNull();
    });
});
