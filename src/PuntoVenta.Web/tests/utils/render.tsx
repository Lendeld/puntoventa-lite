import { render, type RenderOptions } from '@testing-library/react';
import { MantineProvider } from '@mantine/core';
import { QueryClientProvider } from '@tanstack/react-query';
import { NuqsTestingAdapter } from 'nuqs/adapters/testing';
import type { ComponentType, PropsWithChildren, ReactElement } from 'react';
import { makeQueryClient } from '@/lib/query/queryClient';

function AllProviders({ children }: { children: React.ReactNode }) {
  const queryClient = makeQueryClient();

  return (
    <NuqsTestingAdapter hasMemory>
      <QueryClientProvider client={queryClient}>
        {/* env="test" desactiva las transiciones de Mantine: evita timers que
            disparan setState tras el teardown de jsdom ("window is not defined"). */}
        <MantineProvider env="test">{children}</MantineProvider>
      </QueryClientProvider>
    </NuqsTestingAdapter>
  );
}

function customRender(
  ui: ReactElement,
  options?: RenderOptions,
) {
  const Wrapper = options?.wrapper as ComponentType<PropsWithChildren> | undefined;
  const CombinedWrapper = ({ children }: PropsWithChildren) => (
    <AllProviders>
      {Wrapper ? <Wrapper>{children}</Wrapper> : children}
    </AllProviders>
  );

  return render(ui, { ...options, wrapper: CombinedWrapper });
}

export * from '@testing-library/react';
export { customRender as render };
