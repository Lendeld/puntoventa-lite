'use client';

import { ActionIcon, Tooltip, useMantineColorScheme, useComputedColorScheme } from '@mantine/core';
import { IconMoon, IconSun } from '@tabler/icons-react';

interface ColorSchemeToggleProps {
  size?: number;
}

export function ColorSchemeToggle({ size = 20 }: ColorSchemeToggleProps) {
  const { setColorScheme } = useMantineColorScheme();
  const computed = useComputedColorScheme('dark');

  return (
    <Tooltip label={computed === 'dark' ? 'Modo claro' : 'Modo oscuro'} position="bottom">
      <ActionIcon
        onClick={() => setColorScheme(computed === 'dark' ? 'light' : 'dark')}
        variant="subtle"
        size="lg"
        aria-label="Cambiar tema"
      >
        <IconSun size={size} className="hidden dark:block" />
        <IconMoon size={size} className="block dark:hidden" />
      </ActionIcon>
    </Tooltip>
  );
}
