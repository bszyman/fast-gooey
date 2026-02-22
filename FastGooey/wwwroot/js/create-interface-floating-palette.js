(() => {
    const interfaceCreateSelectedClass = 'selectedInterfaceListItem';
    let interfacePaletteOpen = false;

    function getCreateInterfacePalette() {
        return document.getElementById('createInterfaceFloatingPalette');
    }

    function toggleInterfaceCreatePalette() {
        const panel = getCreateInterfacePalette();
        if (!panel) return;

        interfacePaletteOpen = !interfacePaletteOpen;

        if (interfacePaletteOpen) {
            panel.classList.remove('hidden');
            panel.style.animation = 'fadeInEditorPanelNegativeSpaceFrames 0.5s ease-in-out forwards';
        } else {
            panel.style.animation = 'fadeOutEditorPanelNegativeSpaceFrames 0.5s ease-in-out forwards';
            window.setTimeout(() => panel.classList.add('hidden'), 250);
        }
    }

    function clearInterfaceCreateSelection(palette) {
        if (!palette) return;
        palette.querySelectorAll('[data-interface-create-option="true"]').forEach(item => {
            item.classList.remove(interfaceCreateSelectedClass);
        });
    }

    function selectInterfaceCreateOption(button) {
        const palette = getCreateInterfacePalette();
        if (!palette) return;

        clearInterfaceCreateSelection(palette);
        button.classList.add(interfaceCreateSelectedClass);
    }

    function createInterfaceFromPalette(button) {
        const palette = getCreateInterfacePalette();
        if (!palette) return;

        const selected = palette.querySelector(`[data-interface-create-option="true"].${interfaceCreateSelectedClass}`);
        const createUrl = selected?.dataset.interfaceCreateUrl;
        if (!createUrl) return;

        const targetSelector = button?.dataset?.interfaceCreateTarget || '#workspace';
        const token = palette.querySelector('input[name="__RequestVerificationToken"]')?.value;

        if (window.htmx && window.htmx.ajax) {
            const requestOptions = {
                target: targetSelector,
                swap: 'innerHTML'
            };

            if (token) {
                requestOptions.headers = {
                    RequestVerificationToken: token
                };
            }

            window.htmx.ajax('POST', createUrl, {
                ...requestOptions
            });
        } else {
            window.location.href = createUrl;
        }

        toggleInterfaceCreatePalette();
    }

    window.toggleInterfaceCreatePalette = toggleInterfaceCreatePalette;
    window.selectInterfaceCreateOption = selectInterfaceCreateOption;
    window.createInterfaceFromPalette = createInterfaceFromPalette;
})();
