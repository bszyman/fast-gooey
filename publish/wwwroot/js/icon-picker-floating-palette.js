(() => {
    const iconPickerSelectedClass = 'selectedInterfaceListItem';
    const iconBaseUrl = 'https://raw.githubusercontent.com/tailwindlabs/heroicons/master/src/24/outline';

    function getIconPickerPalette() {
        return document.getElementById('iconPalette');
    }

    function getIconPickerIconUrl(iconName) {
        const trimmed = iconName?.trim() || '';
        if (!trimmed) return '';
        return `${iconBaseUrl}/${trimmed}.svg`;
    }

    function formatIconPickerDisplayValue(value) {
        const trimmed = value?.trim() || '';
        if (!trimmed) return 'No icon selected.';
        return trimmed;
    }

    function updateIconPickerDisplay(trigger) {
        const targetId = trigger.dataset.iconPickerTarget;
        const displayId = trigger.dataset.iconPickerDisplay;
        const previewId = trigger.dataset.iconPickerPreview;

        if (!targetId || !displayId || !previewId) return;

        const input = document.getElementById(targetId);
        const display = document.getElementById(displayId);
        const preview = document.getElementById(previewId);
        if (!input || !display || !preview) return;

        const iconName = input.value?.trim() || '';
        if (!iconName) {
            preview.classList.add('hidden');
            preview.removeAttribute('src');
            preview.alt = '';
        } else {
            preview.src = getIconPickerIconUrl(iconName);
            preview.alt = `${iconName} icon`;
            preview.classList.remove('hidden');
        }

        display.textContent = formatIconPickerDisplayValue(iconName);
    }

    function updateIconPickerDisplays(root = document) {
        root.querySelectorAll('[data-icon-picker-trigger="true"]').forEach(updateIconPickerDisplay);
    }

    function clearIconPickerSelection(palette) {
        if (!palette) return;
        palette.querySelectorAll('[data-icon-picker-item="true"]').forEach(item => {
            item.classList.remove(iconPickerSelectedClass);
        });
    }

    function selectIconPickerItem(item) {
        const palette = getIconPickerPalette();
        if (!palette) return;

        clearIconPickerSelection(palette);
        item.classList.add(iconPickerSelectedClass);
    }

    function applyIconPickerSelectionFromValue(value) {
        const palette = getIconPickerPalette();
        if (!palette) return;

        const trimmed = value?.trim() || '';
        clearIconPickerSelection(palette);
        if (!trimmed) return;

        const item = palette.querySelector(`[data-icon-picker-item="true"][data-icon-picker-id="${trimmed}"]`);
        if (item) selectIconPickerItem(item);
    }

    function openIconPicker(trigger) {
        const palette = getIconPickerPalette();
        if (!palette) return;

        const targetId = trigger.dataset.iconPickerTarget;
        const displayId = trigger.dataset.iconPickerDisplay;
        const previewId = trigger.dataset.iconPickerPreview;
        const input = targetId ? document.getElementById(targetId) : null;

        palette.dataset.iconPickerTarget = targetId || '';
        palette.dataset.iconPickerDisplay = displayId || '';
        palette.dataset.iconPickerPreview = previewId || '';
        palette.dataset.iconPickerInputValue = input?.value || '';

        applyIconPickerSelectionFromValue(input?.value || '');

        palette.classList.remove('hidden');
        palette.setAttribute('aria-hidden', 'false');
    }

    function closeIconPicker() {
        const palette = getIconPickerPalette();
        if (!palette) return;

        palette.classList.add('hidden');
        palette.setAttribute('aria-hidden', 'true');
        palette.dataset.iconPickerTarget = '';
        palette.dataset.iconPickerDisplay = '';
        palette.dataset.iconPickerPreview = '';
        palette.dataset.iconPickerInputValue = '';
    }

    function saveIconPickerSelection() {
        const palette = getIconPickerPalette();
        if (!palette) return;

        const targetId = palette.dataset.iconPickerTarget;
        const displayId = palette.dataset.iconPickerDisplay;
        const previewId = palette.dataset.iconPickerPreview;
        if (!targetId || !displayId || !previewId) {
            closeIconPicker();
            return;
        }

        const input = document.getElementById(targetId);
        const display = document.getElementById(displayId);
        const preview = document.getElementById(previewId);
        if (!input || !display || !preview) {
            closeIconPicker();
            return;
        }

        const selectedItem = palette.querySelector(`.${iconPickerSelectedClass}[data-icon-picker-item="true"]`);
        const nextValue = selectedItem?.dataset.iconPickerId || '';

        input.value = nextValue;
        input.dispatchEvent(new Event('input', { bubbles: true }));
        input.dispatchEvent(new Event('change'));

        if (!nextValue) {
            preview.classList.add('hidden');
            preview.removeAttribute('src');
            preview.alt = '';
        } else {
            preview.src = getIconPickerIconUrl(nextValue);
            preview.alt = `${nextValue} icon`;
            preview.classList.remove('hidden');
        }

        display.textContent = formatIconPickerDisplayValue(nextValue);

        closeIconPicker();
    }

    function clearIconPickerSelectionValue() {
        const palette = getIconPickerPalette();
        if (!palette) return;

        const targetId = palette.dataset.iconPickerTarget;
        const displayId = palette.dataset.iconPickerDisplay;
        const previewId = palette.dataset.iconPickerPreview;
        if (!targetId || !displayId || !previewId) {
            closeIconPicker();
            return;
        }

        const input = document.getElementById(targetId);
        const display = document.getElementById(displayId);
        const preview = document.getElementById(previewId);
        if (!input || !display || !preview) {
            closeIconPicker();
            return;
        }

        clearIconPickerSelection(palette);

        input.value = '';
        input.dispatchEvent(new Event('input', { bubbles: true }));
        input.dispatchEvent(new Event('change'));

        preview.classList.add('hidden');
        preview.removeAttribute('src');
        preview.alt = '';
        display.textContent = formatIconPickerDisplayValue('');

        closeIconPicker();
    }

    window.getIconPickerPalette = getIconPickerPalette;
    window.openIconPicker = openIconPicker;
    window.closeIconPicker = closeIconPicker;
    window.saveIconPickerSelection = saveIconPickerSelection;
    window.clearIconPickerSelectionValue = clearIconPickerSelectionValue;
    window.selectIconPickerItem = selectIconPickerItem;
    window.clearIconPickerSelection = clearIconPickerSelection;
    window.updateIconPickerDisplays = updateIconPickerDisplays;
    window.applyIconPickerSelectionFromValue = applyIconPickerSelectionFromValue;
})();
