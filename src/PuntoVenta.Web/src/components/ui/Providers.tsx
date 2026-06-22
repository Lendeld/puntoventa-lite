"use client";

import {
    Badge,
    Button,
    Card,
    type CSSVariablesResolver,
    Drawer,
    HoverCard,
    Input,
    InputWrapper,
    MantineColorsTuple,
    MantineProvider,
    Menu,
    Modal,
    NumberInput,
    Paper,
    PasswordInput,
    Popover,
    SegmentedControl,
    Select,
    Table,
    Tabs,
    TextInput,
    ThemeIcon,
    Tooltip,
    createTheme,
} from "@mantine/core";
import { DatesProvider } from "@mantine/dates";
import { ModalsProvider } from "@mantine/modals";
import { Notifications } from "@mantine/notifications";
import { NavigationProgress } from "@mantine/nprogress";
import "@mantine/nprogress/styles.css";
import { RouterProgressSync } from "@ui/RouterProgressSync";
import { QueryProvider } from "@/components/providers/QueryProvider";
import "@lib/utils/silenceRechartsWarning";

const cssVariablesResolver: CSSVariablesResolver = () => ({
    variables: {},
    light: { "--mantine-color-body": "#f4f6f8" },
    dark: { "--mantine-color-body": "#26282d" },
});

const accentPV: MantineColorsTuple = [
    "#e7f8f1",
    "#cfeee2",
    "#a5dfc8",
    "#79cfad",
    "#4fc095",
    "#2ea581",
    "#148a65",
    "#0e6b4f",
    "#084f3b",
    "#04342c",
];

const SOFT_TRANSITION = {
    duration: 220,
    timingFunction: "cubic-bezier(0.2, 0.8, 0.2, 1)",
};

const theme = createTheme({
    primaryColor: "accentPV",
    primaryShade: 6,
    autoContrast: true,
    luminanceThreshold: 0.3,
    colors: {
        accentPV,
    },
    defaultRadius: "md",
    fontFamily: "var(--font-geist-sans), sans-serif",
    fontFamilyMonospace: "var(--font-geist-mono), monospace",
    headings: {
        fontFamily: "var(--font-geist-sans), sans-serif",
        fontWeight: "600",
    },
    components: {
        InputWrapper: InputWrapper.extend({
            classNames: {
                label: "text-sm font-semibold text-theme-text-muted mb-1.5 tracking-wide",
            },
        }),
        Input: Input.extend({
            classNames: {
                input: "rounded-sm bg-theme-surface text-theme-text placeholder:text-theme-text-dim transition-shadow duration-(--dur-fast) ease-soft focus-within:shadow-(--ring-accent) data-[error=true]:focus-within:shadow-none",
                section: "text-theme-text-dim",
            },
        }),
        TextInput: TextInput.extend({
            classNames: {
                label: "text-sm font-semibold text-theme-text-muted mb-1.5 tracking-wide",
                input: "rounded-sm bg-theme-surface text-theme-text placeholder:text-theme-text-dim transition-shadow duration-(--dur-fast) ease-soft focus-within:shadow-(--ring-accent) data-[error=true]:focus-within:shadow-none",
                section: "text-theme-text-dim",
            },
        }),
        PasswordInput: PasswordInput.extend({
            classNames: {
                label: "text-sm font-semibold text-theme-text-muted mb-1.5 tracking-wide",
                input: "rounded-sm bg-theme-surface transition-shadow duration-(--dur-fast) ease-soft focus-within:shadow-(--ring-accent) data-[error=true]:focus-within:shadow-none",
                innerInput: "text-theme-text placeholder:text-theme-text-dim",
                section: "text-theme-text-dim",
            },
        }),
        Button: Button.extend({
            defaultProps: {
                color: "accentPV",
            },
            classNames: {
                root: "font-semibold shadow-xs transition-[transform,box-shadow,background-color] duration-(--dur-fast) ease-soft hover:-translate-y-px hover:shadow-soft active:translate-y-0 focus-visible:shadow-(--ring-accent)",
            },
        }),
        SegmentedControl: SegmentedControl.extend({
            defaultProps: {
                color: "accentPV",
            },
        }),
        Select: Select.extend({
            classNames: {
                label: "text-sm font-semibold text-theme-text-muted mb-1.5 tracking-wide",
                input: "rounded-sm bg-theme-surface text-theme-text placeholder:text-theme-text-dim transition-shadow duration-(--dur-fast) ease-soft focus-within:shadow-(--ring-accent) data-[error=true]:focus-within:shadow-none",
                section: "text-theme-text-dim",
                dropdown:
                    "bg-theme-surface border border-theme-border-soft rounded-md shadow-elevated dark:bg-theme-surface-2",
                option: "text-theme-text rounded-xs hover:bg-theme-accent-soft data-[hovered=true]:bg-theme-accent-soft dark:hover:bg-theme-surface-3 dark:data-[hovered=true]:bg-theme-surface-3",
            },
        }),
        Table: Table.extend({
            classNames: {
                table: "bg-theme-surface",
                thead: "bg-theme-surface border-b border-theme-border-soft",
                th: "text-theme-text-muted font-semibold text-xs uppercase tracking-wide",
                td: "text-theme-text",
                tbody: "divide-y divide-theme-border-soft",
                tr: "transition-colors duration-(--dur-fast) hover:bg-theme-surface-2 dark:hover:bg-theme-surface-2",
            },
        }),
        Tabs: Tabs.extend({
            classNames: {
                root: "h-full",
                list: "w-full gap-2 border-0 p-2 before:border-0",
                tab: "relative border-0 rounded-theme px-2 py-2 text-left text-theme-text-muted transition-all duration-(--dur-fast) ease-soft hover:bg-theme-surface-3 hover:text-theme-text data-[active=true]:bg-theme-accent-soft data-[active=true]:text-theme-text data-[active=true]:font-semibold dark:hover:bg-theme-surface-2 dark:data-[active=true]:text-theme-text/90",
                tabLabel: "min-w-0 flex-1 text-left",
            },
        }),
        ThemeIcon: ThemeIcon.extend({
            classNames: {
                root: "bg-theme-accent-soft text-theme",
            },
        }),
        HoverCard: HoverCard.extend({
            classNames: {
                dropdown:
                    "bg-theme-surface border border-theme-border-soft text-theme-text rounded-md shadow-elevated dark:bg-theme-surface-2",
                arrow: "border-theme bg-theme-surface dark:bg-theme-surface-2",
            },
        }),
        Menu: Menu.extend({
            classNames: {
                dropdown:
                    "bg-theme-surface border border-theme-border-soft rounded-md shadow-elevated dark:bg-theme-surface-2",
                item: "text-theme-text rounded-xs transition-colors duration-(--dur-fast) hover:bg-theme-accent-soft data-[hovered=true]:bg-theme-accent-soft dark:hover:bg-theme-surface-3 dark:data-[hovered=true]:bg-theme-surface-3",
            },
        }),
        Tooltip: Tooltip.extend({
            defaultProps: {
                transitionProps: SOFT_TRANSITION,
                openDelay: 120,
            },
            classNames: {
                tooltip:
                    "border border-theme-border-soft bg-theme-surface-2 text-theme-text rounded-xs text-xs shadow-soft dark:bg-theme-surface-3",
                arrow: "border border-theme-border-soft bg-theme-surface-2 dark:bg-theme-surface-3",
            },
        }),
        NumberInput: NumberInput.extend({
            defaultProps: {
                thousandSeparator: ",",
                decimalSeparator: ".",
                allowNegative: false,
                hideControls: true,
                rightSectionWidth: 0,
                rightSection: null,
            },
            classNames: {
                input: "text-right rounded-sm tabular-nums transition-shadow duration-(--dur-fast) ease-soft focus-within:shadow-(--ring-accent)",
            },
        }),
        Popover: Popover.extend({
            classNames: {
                dropdown:
                    "bg-theme-surface border border-theme-border-soft text-theme-text rounded-md shadow-elevated dark:bg-theme-surface-2",
                arrow: "border-theme bg-theme-surface dark:bg-theme-surface-2",
            },
        }),
        Card: Card.extend({
            classNames: {
                root: "rounded-lg border border-theme-border-soft bg-theme-surface transition-colors duration-(--dur-med) ease-soft",
            },
        }),
        Paper: Paper.extend({
            classNames: {
                root: "bg-theme-surface dark:bg-theme-surface",
            },
        }),
        Modal: Modal.extend({
            defaultProps: {
                transitionProps: SOFT_TRANSITION,
                overlayProps: { backgroundOpacity: 0.55, blur: 4 },
                centered: true,
                radius: "var(--radius-lg)",
            },
            classNames: {
                content:
                    "rounded-lg border border-theme-border-soft bg-theme-surface shadow-modal dark:bg-theme-surface-2 dark:border-theme",
                header:
                    "bg-theme-surface dark:bg-theme-surface-2 border-b border-theme-border-soft dark:border-theme pb-3",
                title: "font-display text-xl text-theme-text",
                body: "pt-5",
                overlay: "backdrop-blur-sm",
            },
        }),
        Drawer: Drawer.extend({
            defaultProps: {
                transitionProps: SOFT_TRANSITION,
                overlayProps: { backgroundOpacity: 0.5, blur: 4 },
            },
            classNames: {
                content:
                    "bg-theme-surface border-l border-theme-border-soft shadow-modal dark:bg-theme-surface-2 dark:border-theme",
                header:
                    "bg-theme-surface dark:bg-theme-surface-2 border-b border-theme-border-soft dark:border-theme pb-3",
                title: "font-display text-xl text-theme-text",
                body: "pt-5",
                overlay: "backdrop-blur-sm",
            },
        }),
        Badge: Badge.extend({
            classNames: {
                root: "font-medium tracking-wide rounded-pill",
            },
        }),
    },
});

export function Providers({ children }: { children: React.ReactNode }) {
    return (
        <QueryProvider>
            <MantineProvider
                theme={theme}
                cssVariablesResolver={cssVariablesResolver}
                defaultColorScheme="dark"
            >
                <DatesProvider
                    settings={{
                        locale: "es",
                        firstDayOfWeek: 0,
                    }}
                >
                    <ModalsProvider
                        modalProps={{
                            centered: true,
                            radius: "var(--radius-lg)",
                            overlayProps: {
                                backgroundOpacity: 0.55,
                                blur: 4,
                            },
                            transitionProps: SOFT_TRANSITION,
                            classNames: {
                                content:
                                    "rounded-lg border border-theme-border-soft bg-theme-surface p-2 shadow-modal dark:bg-theme-surface-2 dark:border-theme",
                                header: "relative bg-transparent border-b border-theme-border-soft dark:border-theme",
                                title: "font-display text-xl text-theme-text",
                                overlay: "backdrop-blur-sm",
                            },
                        }}
                    >
                        <NavigationProgress />
                        <RouterProgressSync />
                        <Notifications
                            position="top-right"
                            classNames={{
                                notification:
                                    "rounded-md border border-theme-border-soft bg-theme-surface shadow-elevated dark:bg-theme-surface-2 dark:border-theme",
                            }}
                        />
                        {children}
                    </ModalsProvider>
                </DatesProvider>
            </MantineProvider>
        </QueryProvider>
    );
}
