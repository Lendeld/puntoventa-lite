"use client";

import { useState } from "react";
import { Box, Center, PasswordInput, Popover, Progress, Text } from "@mantine/core";
import type { PasswordInputProps } from "@mantine/core";
import { IconCheck, IconX } from "@tabler/icons-react";

const requirements = [
    { re: /[0-9]/, label: "Incluye un número" },
    { re: /[a-z]/, label: "Incluye letra minúscula" },
    { re: /[A-Z]/, label: "Incluye letra mayúscula" },
    { re: /[$&+,:;=?@#|'<>.^*()%!-]/, label: "Incluye símbolo especial" },
];

function getStrength(password: string) {
    let multiplier = password.length >= 8 ? 0 : 1;
    requirements.forEach((req) => {
        if (!req.re.test(password)) multiplier += 1;
    });
    return Math.max(100 - (100 / (requirements.length + 1)) * multiplier, 10);
}

function PasswordRequirement({ meets, label }: { meets: boolean; label: string }) {
    return (
        <Text
            component="div"
            c={meets ? "teal" : "red"}
            style={{ display: "flex", alignItems: "center" }}
            mt={7}
            size="sm"
        >
            <Center>
                {meets ? <IconCheck size={14} stroke={1.5} /> : <IconX size={14} stroke={1.5} />}
            </Center>
            <Box ml={10}>{label}</Box>
        </Text>
    );
}

export function PasswordStrengthInput(props: PasswordInputProps) {
    const [popoverOpened, setPopoverOpened] = useState(false);
    const value = (props.value as string) ?? "";
    const strength = getStrength(value);
    const color = strength === 100 ? "teal" : strength > 50 ? "yellow" : "red";

    return (
        <Popover
            opened={popoverOpened}
            position="bottom"
            width="target"
            transitionProps={{ transition: "pop" }}
        >
            <Popover.Target>
                <div
                    onFocusCapture={() => setPopoverOpened(true)}
                    onBlurCapture={() => setPopoverOpened(false)}
                >
                    <PasswordInput {...props} />
                </div>
            </Popover.Target>
            <Popover.Dropdown>
                <Progress color={color} value={strength} size={5} mb="xs" />
                <PasswordRequirement meets={value.length >= 8} label="Al menos 8 caracteres" />
                {requirements.map((req) => (
                    <PasswordRequirement key={req.label} meets={req.re.test(value)} label={req.label} />
                ))}
            </Popover.Dropdown>
        </Popover>
    );
}
