document.addEventListener("DOMContentLoaded", () => {
    const modal = document.getElementById("cvIntelligenceModal");
    const openButton = document.getElementById("openCvIntelligence");
    const closeButton = document.getElementById("closeCvIntelligence");
    const analysis = document.getElementById("cvIntelligenceAnalysis");
    const plans = document.getElementById("cvIntelligencePlans");
    const status = document.getElementById("cvIntelligenceStatus");
    const progress = document.getElementById("cvIntelligenceProgress");
    const form = document.getElementById("jobApplicationForm");
    const submitButton = document.getElementById("submitApplicationButton");

    let analysisTimer = null;

    const closeModal = () => {
        if (!modal) return;

        modal.classList.remove("open");
        modal.setAttribute("aria-hidden", "true");
        document.body.style.overflow = "";

        if (analysisTimer) {
            window.clearInterval(analysisTimer);
            analysisTimer = null;
        }
    };

    const openModal = () => {
        if (!modal || !analysis || !plans || !status || !progress) return;

        modal.classList.add("open");
        modal.setAttribute("aria-hidden", "false");
        document.body.style.overflow = "hidden";

        analysis.hidden = false;
        plans.classList.remove("show");
        progress.style.width = "12%";

        const steps = [
            { text: "Reading your primary WorkHub CV...", progress: 28 },
            { text: "Comparing skills with the vacancy...", progress: 50 },
            { text: "Checking experience and education...", progress: 70 },
            { text: "Reviewing job relevance...", progress: 88 },
            { text: "Your CV Intelligence options are ready.", progress: 100 }
        ];

        let index = 0;
        status.textContent = steps[0].text;
        progress.style.width = `${steps[0].progress}%`;

        if (analysisTimer) window.clearInterval(analysisTimer);

        analysisTimer = window.setInterval(() => {
            index += 1;

            if (index < steps.length) {
                status.textContent = steps[index].text;
                progress.style.width = `${steps[index].progress}%`;
                return;
            }

            window.clearInterval(analysisTimer);
            analysisTimer = null;

            window.setTimeout(() => {
                analysis.hidden = true;
                plans.classList.add("show");
            }, 300);
        }, 550);
    };

    openButton?.addEventListener("click", openModal);
    closeButton?.addEventListener("click", closeModal);

    modal?.addEventListener("click", event => {
        if (event.target === modal) closeModal();
    });

    document.addEventListener("keydown", event => {
        if (event.key === "Escape" && modal?.classList.contains("open")) {
            closeModal();
        }
    });

    form?.addEventListener("submit", () => {
        if (!submitButton || !form.checkValidity()) return;

        submitButton.disabled = true;
        submitButton.textContent = "Submitting Application...";
    });
});
