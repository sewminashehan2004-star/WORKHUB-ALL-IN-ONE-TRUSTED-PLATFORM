document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll("[data-password-target]").forEach(button => {
        button.addEventListener("click", () => {
            const targetId = button.getAttribute("data-password-target");
            const input = targetId ? document.getElementById(targetId) : null;

            if (!(input instanceof HTMLInputElement)) {
                return;
            }

            const showPassword = input.type === "password";
            input.type = showPassword ? "text" : "password";
            button.classList.toggle("active", showPassword);
            button.setAttribute("aria-label", showPassword ? "Hide password" : "Show password");
        });
    });
});
