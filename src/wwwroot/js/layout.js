const CHAT_BREAKPOINT = 1000; // match CSS @media (max-width:1000px)

function evaluate(dotnetRef) {
    const collapsible = window.innerWidth <= CHAT_BREAKPOINT;
    dotnetRef.invokeMethodAsync('SetChatCollapsible', collapsible);
}

export function initLayout(dotnetRef) {
    function handler() { evaluate(dotnetRef); }
    window.addEventListener('resize', handler, { passive: true });
    // Initial
    evaluate(dotnetRef);
    return {
        dispose() {
            window.removeEventListener('resize', handler);
        }
    };
}
