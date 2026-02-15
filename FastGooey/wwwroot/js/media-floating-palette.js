(() => {
    const mediaPickerSchemePrefix = 'fastgooey:media:';
    const mediaPickerSelectedClass = 'selectedInterfaceListItem';

    function getMediaPickerPalette() {
        return document.getElementById('mediaPalette');
    }

    function getMediaPickerItemMap() {
        const palette = getMediaPickerPalette();
        const map = new Map();

        if (!palette) return map;

        palette.querySelectorAll('[data-media-picker-item="true"]').forEach(item => {
            const sourceId = item.dataset.mediaSourceId;
            const path = item.dataset.mediaPath;
            if (!sourceId || !path) return;

            const key = `${sourceId}:${path}`;
            map.set(key, {
                label: item.dataset.mediaLabel || 'Untitled',
                sourceName: item.dataset.mediaSourceName || ''
            });
        });

        return map;
    }

    function parseMediaPickerValue(value) {
        const trimmed = value?.trim() || '';
        if (!trimmed.toLowerCase().startsWith(mediaPickerSchemePrefix)) return null;

        const remainder = trimmed.slice(mediaPickerSchemePrefix.length);
        const separatorIndex = remainder.indexOf(':');
        if (separatorIndex < 0) return null;

        const sourceId = remainder.slice(0, separatorIndex);
        const encodedPath = remainder.slice(separatorIndex + 1);
        if (!sourceId || !encodedPath) return null;

        let decodedPath = '';
        try {
            decodedPath = decodeURIComponent(encodedPath);
        } catch {
            decodedPath = encodedPath;
        }

        return { sourceId, path: decodedPath };
    }

    function formatMediaPickerDisplayValue(value) {
        const trimmed = value?.trim() || '';
        if (!trimmed) return 'Select media...';

        if (!trimmed.toLowerCase().startsWith(mediaPickerSchemePrefix)) {
            return trimmed;
        }

        const parsed = parseMediaPickerValue(trimmed);
        if (!parsed) return 'Unknown FastGooey media';

        const itemMap = getMediaPickerItemMap();
        const match = itemMap.get(`${parsed.sourceId}:${parsed.path}`);
        if (!match) return 'Unknown FastGooey media';

        const sourceSuffix = match.sourceName ? ` (${match.sourceName})` : '';
        return `${match.label}${sourceSuffix}`;
    }

    function updateMediaPickerDisplay(trigger) {
        const targetId = trigger.dataset.mediaPickerTarget;
        const displayId = trigger.dataset.mediaPickerDisplay;
        if (!targetId || !displayId) return;

        const input = document.getElementById(targetId);
        const display = document.getElementById(displayId);
        if (!input || !display) return;

        display.textContent = formatMediaPickerDisplayValue(input.value);
    }

    function updateMediaPickerDisplays(root = document) {
        root.querySelectorAll('[data-media-picker-trigger="true"]').forEach(updateMediaPickerDisplay);
    }

    function clearMediaPickerSelection(palette) {
        if (!palette) return;
        palette.querySelectorAll('[data-media-picker-item="true"]').forEach(item => {
            item.classList.remove(mediaPickerSelectedClass);
        });
    }

    function selectMediaPickerItem(item) {
        const palette = getMediaPickerPalette();
        if (!palette) return;
        clearMediaPickerSelection(palette);
        item.classList.add(mediaPickerSelectedClass);
    }

    function applyMediaPickerSelectionFromValue(value) {
        const palette = getMediaPickerPalette();
        if (!palette) return;

        const customUrlInput = document.getElementById('mediaPickerCustomUrl');
        const trimmed = value?.trim() || '';

        clearMediaPickerSelection(palette);

        if (!trimmed) {
            if (customUrlInput) customUrlInput.value = '';
            return;
        }

        const parsed = parseMediaPickerValue(trimmed);
        if (parsed) {
            const items = palette.querySelectorAll('[data-media-picker-item="true"]');
            for (const item of items) {
                if (item.dataset.mediaSourceId === parsed.sourceId && item.dataset.mediaPath === parsed.path) {
                    selectMediaPickerItem(item);
                    if (customUrlInput) customUrlInput.value = '';
                    return;
                }
            }

            if (customUrlInput) customUrlInput.value = '';
            return;
        }

        if (customUrlInput) customUrlInput.value = trimmed;
    }

    function openMediaPicker(trigger) {
        const palette = getMediaPickerPalette();
        if (!palette) return;

        const targetId = trigger.dataset.mediaPickerTarget;
        const displayId = trigger.dataset.mediaPickerDisplay;
        const input = targetId ? document.getElementById(targetId) : null;

        palette.dataset.mediaPickerTarget = targetId || '';
        palette.dataset.mediaPickerDisplay = displayId || '';
        palette.dataset.mediaPickerInputValue = input?.value || '';

        applyMediaPickerSelectionFromValue(input?.value || '');

        const contentSelector = palette.querySelector('#contentSelector');
        if (contentSelector) {
            if (window.htmx) {
                window.htmx.trigger(contentSelector, 'load-media-palette');
            } else {
                contentSelector.dispatchEvent(new Event('load-media-palette'));
            }
        }

        palette.classList.remove('hidden');
        palette.setAttribute('aria-hidden', 'false');
    }

    function closeMediaPicker() {
        const palette = getMediaPickerPalette();
        if (!palette) return;

        palette.classList.add('hidden');
        palette.setAttribute('aria-hidden', 'true');
        palette.dataset.mediaPickerTarget = '';
        palette.dataset.mediaPickerDisplay = '';
        palette.dataset.mediaPickerInputValue = '';
    }

    function saveMediaPickerSelection() {
        const palette = getMediaPickerPalette();
        if (!palette) return;

        const targetId = palette.dataset.mediaPickerTarget;
        const displayId = palette.dataset.mediaPickerDisplay;
        if (!targetId || !displayId) {
            closeMediaPicker();
            return;
        }

        const input = document.getElementById(targetId);
        const display = document.getElementById(displayId);
        if (!input || !display) {
            closeMediaPicker();
            return;
        }

        const customUrlInput = document.getElementById('mediaPickerCustomUrl');
        const selectedItem = palette.querySelector(`.${mediaPickerSelectedClass}[data-media-picker-item="true"]`);
        let nextValue = '';

        if (customUrlInput && customUrlInput.value.trim()) {
            nextValue = customUrlInput.value.trim();
        } else if (selectedItem) {
            const sourceId = selectedItem.dataset.mediaSourceId || '';
            const path = selectedItem.dataset.mediaPath || '';
            if (sourceId && path) {
                nextValue = `${mediaPickerSchemePrefix}${sourceId}:${encodeURIComponent(path)}`;
            }
        }

        input.value = nextValue;
        input.dispatchEvent(new Event('input', { bubbles: true }));
        input.dispatchEvent(new Event('change', { bubbles: true }));
        display.textContent = formatMediaPickerDisplayValue(nextValue);

        closeMediaPicker();
    }

    window.getMediaPickerPalette = getMediaPickerPalette;
    window.openMediaPicker = openMediaPicker;
    window.closeMediaPicker = closeMediaPicker;
    window.saveMediaPickerSelection = saveMediaPickerSelection;
    window.selectMediaPickerItem = selectMediaPickerItem;
    window.clearMediaPickerSelection = clearMediaPickerSelection;
    window.updateMediaPickerDisplays = updateMediaPickerDisplays;
    window.applyMediaPickerSelectionFromValue = applyMediaPickerSelectionFromValue;
})();
