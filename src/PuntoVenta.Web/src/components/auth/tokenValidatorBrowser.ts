import { ROUTES } from "@lib/constants/routes.constants";

export const tokenValidatorBrowser = {
    redirectToLogout() {
        window.location.replace(ROUTES.API_LOGOUT);
    },
    reloadPage() {
        window.location.reload();
    },
};
