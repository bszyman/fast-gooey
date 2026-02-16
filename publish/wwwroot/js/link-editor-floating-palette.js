(() => {
    const linkEditorSchemePrefix = 'fastgooey:';
    const linkEditorSelectedClass = 'selectedInterfaceListItem';

    function getLinkEditorPalette() {
        return document.getElementById('linkEditorPalette');
    }

    function getLinkEditorNodeMap() {
        const palette = getLinkEditorPalette();
        const map = new Map();

        if (!palette) return map;

        palette.querySelectorAll('[data-link-editor-item="true"]').forEach(item => {
            const id = item.dataset.linkEditorId;
            if (!id) return;

            map.set(id, {
                label: item.dataset.linkEditorLabel || 'Untitled',
                platform: item.dataset.linkEditorPlatform || ''
            });
        });

        return map;
    }

    function formatLinkEditorDisplayValue(value) {
        const trimmed = value?.trim() || '';
        if (!trimmed) return 'No link selected.';

        if (!trimmed.toLowerCase().startsWith(linkEditorSchemePrefix)) {
            return trimmed;
        }

        const id = trimmed.slice(linkEditorSchemePrefix.length);
        const nodeMap = getLinkEditorNodeMap();
        const match = nodeMap.get(id);
        if (!match) return 'Unknown FastGooey link';

        const platformSuffix = match.platform ? ` (${match.platform})` : '';
        return `${match.label}${platformSuffix}`;
    }

    function updateLinkEditorDisplay(trigger) {
        const targetId = trigger.dataset.linkEditorTarget;
        const displayId = trigger.dataset.linkEditorDisplay;
        if (!targetId || !displayId) return;

        const input = document.getElementById(targetId);
        const display = document.getElementById(displayId);
        if (!input || !display) return;

        display.textContent = formatLinkEditorDisplayValue(input.value);
    }

    function updateLinkEditorDisplays(root = document) {
        root.querySelectorAll('[data-link-editor-trigger="true"]').forEach(updateLinkEditorDisplay);
    }

    function clearLinkEditorSelection(palette) {
        if (!palette) return;
        palette.querySelectorAll('[data-link-editor-item="true"]').forEach(item => {
            item.classList.remove(linkEditorSelectedClass);
        });
    }

    function selectLinkEditorItem(item) {
        const palette = getLinkEditorPalette();
        if (!palette) return;
        clearLinkEditorSelection(palette);
        item.classList.add(linkEditorSelectedClass);
    }

    function applyLinkEditorSelectionFromValue(value) {
        const palette = getLinkEditorPalette();
        if (!palette) return;

        const customUrlInput = document.getElementById('linkEditorCustomUrl');
        const trimmed = value?.trim() || '';

        clearLinkEditorSelection(palette);

        if (!trimmed) {
            if (customUrlInput) customUrlInput.value = '';
            return;
        }

        if (trimmed.toLowerCase().startsWith(linkEditorSchemePrefix)) {
            const id = trimmed.slice(linkEditorSchemePrefix.length);
            const item = palette.querySelector(`[data-link-editor-item="true"][data-link-editor-id="${id}"]`);
            if (item) {
                selectLinkEditorItem(item);
                if (customUrlInput) customUrlInput.value = '';
                return;
            }

            if (customUrlInput) customUrlInput.value = '';
            return;
        }

        if (customUrlInput) customUrlInput.value = trimmed;
    }

    function openLinkEditor(trigger) {
        const palette = getLinkEditorPalette();
        if (!palette) return;

        const targetId = trigger.dataset.linkEditorTarget;
        const displayId = trigger.dataset.linkEditorDisplay;
        const input = targetId ? document.getElementById(targetId) : null;
        const customUrlInput = document.getElementById('linkEditorCustomUrl');

        palette.dataset.linkEditorTarget = targetId || '';
        palette.dataset.linkEditorDisplay = displayId || '';
        palette.dataset.linkEditorInputValue = input?.value || '';

        applyLinkEditorSelectionFromValue(input?.value || '');

        const contentSelector = palette.querySelector('#contentSelector');
        if (contentSelector) {
            if (window.htmx) {
                window.htmx.trigger(contentSelector, 'load-link-editor');
            } else {
                contentSelector.dispatchEvent(new Event('load-link-editor'));
            }
        }

        palette.classList.remove('hidden');
        palette.setAttribute('aria-hidden', 'false');
    }

    function closeLinkEditor() {
        const palette = getLinkEditorPalette();
        if (!palette) return;

        palette.classList.add('hidden');
        palette.setAttribute('aria-hidden', 'true');
        palette.dataset.linkEditorTarget = '';
        palette.dataset.linkEditorDisplay = '';
        palette.dataset.linkEditorInputValue = '';
    }

    function saveLinkEditorSelection() {
        const palette = getLinkEditorPalette();
        if (!palette) return;

        const targetId = palette.dataset.linkEditorTarget;
        const displayId = palette.dataset.linkEditorDisplay;
        if (!targetId || !displayId) {
            closeLinkEditor();
            return;
        }

        const input = document.getElementById(targetId);
        const display = document.getElementById(displayId);
        if (!input || !display) {
            closeLinkEditor();
            return;
        }

        const customUrlInput = document.getElementById('linkEditorCustomUrl');
        const selectedItem = palette.querySelector(`.${linkEditorSelectedClass}[data-link-editor-item="true"]`);
        let nextValue = '';

        if (customUrlInput && customUrlInput.value.trim()) {
            nextValue = customUrlInput.value.trim();
        } else if (selectedItem) {
            nextValue = `${linkEditorSchemePrefix}${selectedItem.dataset.linkEditorId}`;
        }

        input.value = nextValue;
        input.dispatchEvent(new Event('input', { bubbles: true }));
        display.textContent = formatLinkEditorDisplayValue(nextValue);

        closeLinkEditor();
    }

    function clearLinkEditorSelectionValue() {
        const palette = getLinkEditorPalette();
        if (!palette) return;

        const targetId = palette.dataset.linkEditorTarget;
        const displayId = palette.dataset.linkEditorDisplay;
        if (!targetId || !displayId) {
            closeLinkEditor();
            return;
        }

        const input = document.getElementById(targetId);
        const display = document.getElementById(displayId);
        if (!input || !display) {
            closeLinkEditor();
            return;
        }

        const customUrlInput = document.getElementById('linkEditorCustomUrl');
        if (customUrlInput) customUrlInput.value = '';

        clearLinkEditorSelection(palette);

        input.value = '';
        input.dispatchEvent(new Event('input', { bubbles: true }));
        display.textContent = formatLinkEditorDisplayValue('');

        closeLinkEditor();
    }

    window.getLinkEditorPalette = getLinkEditorPalette;
    window.openLinkEditor = openLinkEditor;
    window.closeLinkEditor = closeLinkEditor;
    window.saveLinkEditorSelection = saveLinkEditorSelection;
    window.clearLinkEditorSelectionValue = clearLinkEditorSelectionValue;
    window.selectLinkEditorItem = selectLinkEditorItem;
    window.clearLinkEditorSelection = clearLinkEditorSelection;
    window.updateLinkEditorDisplays = updateLinkEditorDisplays;
    window.applyLinkEditorSelectionFromValue = applyLinkEditorSelectionFromValue;
})();
