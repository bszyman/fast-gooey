// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
let editorOpen = false;

function toggleEditor() {
    const panel = document.getElementById('editorPanel');
    const negativeSpace = document.getElementById('editorPanelNegativeSpace');

    editorOpen = !editorOpen;

    if (editorOpen) {
        negativeSpace.style.animation = 'fadeInEditorPanelNegativeSpaceFrames 0.5s ease-in-out forwards';
        panel.style.animation = 'slideInEditorPanelFrames 0.5s ease-in-out forwards';
    } else {
        negativeSpace.style.animation = 'fadeOutEditorPanelNegativeSpaceFrames 0.5s ease-in-out forwards';
        panel.style.animation = 'slideOutEditorPanelFrames 0.5s ease-in-out forwards';
    }
}

function toggleSubmenu(button) {
    const ul = button.closest('li').querySelector('ul');

    if (ul.style.maxHeight && ul.style.maxHeight !== '0px') {
        // Collapse
        ul.style.maxHeight = '0px';
        ul.style.opacity = '0';
    } else {
        // Expand
        ul.style.maxHeight = ul.scrollHeight + 'px';
        ul.style.opacity = '1';
    }
}

window.addEventListener('openEditor', () => {
    if (!editorOpen) toggleEditor();
});

window.addEventListener('closeEditor', () => {
    if (editorOpen) toggleEditor();
});

function changeSection(sender) {
    const activeButton = sender.parentNode.querySelector('.activeSectionButton');
    
    if (activeButton) {
        if (sender === activeButton) return;
        
        activeButton.classList.remove('activeSectionButton');
    }

    sender.classList.add('activeSectionButton');
}

function changeInterfaceListSelection(sender) {
    const list = document.getElementById('interfaceList');
    list.querySelectorAll('.selectedInterfaceListItem').forEach(item => item.classList.remove('selectedInterfaceListItem'));

    sender.classList.add('selectedInterfaceListItem');
}

function deleteSelectedInterface(deleteUrlBase) {
    const list = document.getElementById('interfaceList');
    const selected = list?.querySelector('.selectedInterfaceListItem');
    const interfaceId = selected?.dataset?.interfaceId;

    if (!interfaceId) {
        return;
    }

    if (!window.confirm('Delete this interface? This cannot be undone.')) {
        return;
    }

    htmx.ajax('DELETE', `${deleteUrlBase}/${interfaceId}`, {
        target: '#workspace',
        swap: 'innerHTML'
    });
}

document.addEventListener('htmx:afterRequest', function(event) {
    if (event.detail.successful) {
        let elt = event.detail.elt;
        // If the element that triggered the request is a form, use it.
        // If it's inside a form, find the closest form.
        let form = elt.closest('form');

        // Check the request configuration. HTMX detail.elt might be the element that triggered the event,
        // but hx-post might be on the form. HTMX also provides requestConfig.
        const requestConfig = event.detail.requestConfig;
        const isHtmxRequest = requestConfig && (requestConfig.verb === 'post' || requestConfig.verb === 'put' || requestConfig.verb === 'patch');

        // Only apply to forms that are likely auto-saving or using htmx explicitly
        if (form && isHtmxRequest) {
            // Exclude specific forms if needed by checking their ID or parent
            if (form.id === 'loginPanel' || form.closest('#loginPanel')) return;

            // In some cases (like WorkspaceManagement.cshtml), the form might be swapped out and replaced.
            // htmx:afterRequest fires after the swap, so the 'form' variable might point to a detached element.
            // We should try to find the inputs in the new content (event.detail.target).
            
            let target = event.detail.target;
            let inputs = [];
            
            if (target && target.querySelectorAll) {
                // If the target itself is an input/select/textarea, include it
                if (target.matches('input, textarea, select')) {
                    inputs.push(target);
                }
                // Also find all inputs within the target
                inputs = inputs.concat(Array.from(target.querySelectorAll('input, textarea, select')));
            }

            // Fallback to form if target didn't yield inputs (and form is still in DOM or we want to try anyway)
            if (inputs.length === 0 && form) {
                inputs = form.querySelectorAll('input, textarea, select');
            }
            
            inputs.forEach(input => {
                input.classList.add('saved');
            });
        }
    }
});

document.addEventListener('input', function(event) {
    const target = event.target;
    if (target.matches('input, textarea, select')) {
        if (target.classList.contains('saved')) {
            target.classList.remove('saved');
        }
    }
});

function updateMediaSourceFields(root = document) {
    const select = root.querySelector('[data-media-source-type-select="true"]');
    if (!select) return;

    const selectedType = select.value;
    root.querySelectorAll('.media-source-fields').forEach(section => {
        const sectionType = section.dataset.mediaSourceType;
        section.style.display = sectionType === selectedType ? 'block' : 'none';
    });
}

function ensureContentOrderingScript(root = document) {
    if (!root.querySelector('[data-content-order-container="true"]')) return;
    if (document.querySelector('script[data-content-ordering-script="true"]')) return;

    const script = document.createElement('script');
    script.src = '/js/content-ordering.js';
    script.defer = true;
    script.dataset.contentOrderingScript = 'true';
    document.body.appendChild(script);
}

const linkEditorSchemePrefix = 'fastgooey:';
const linkEditorSelectedClass = 'selectedInterfaceListItem';
const mediaPickerSchemePrefix = 'fastgooey:media:';
const mediaPickerSelectedClass = 'selectedInterfaceListItem';

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
    if (!trimmed) return 'Select link...';

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
    display.textContent = formatMediaPickerDisplayValue(nextValue);

    closeMediaPicker();
}

document.addEventListener('change', function(event) {
    const target = event.target;
    if (target.matches('[data-media-source-type-select="true"]')) {
        updateMediaSourceFields(target.closest('form') || document);
    }
});

document.addEventListener('htmx:afterSwap', function(event) {
    updateMediaSourceFields(event.detail.target || document);
    ensureContentOrderingScript(event.detail.target || document);
    updateLinkEditorDisplays(event.detail.target || document);
    updateMediaPickerDisplays(event.detail.target || document);

    const target = event.detail.target;
    if (target && target.id === 'contentSelector') {
        const linkPalette = getLinkEditorPalette();
        if (linkPalette && linkPalette.contains(target) && linkPalette.dataset.linkEditorInputValue) {
            applyLinkEditorSelectionFromValue(linkPalette.dataset.linkEditorInputValue);
            updateLinkEditorDisplays(document);
        }

        const mediaPalette = getMediaPickerPalette();
        if (mediaPalette && mediaPalette.contains(target) && mediaPalette.dataset.mediaPickerInputValue) {
            applyMediaPickerSelectionFromValue(mediaPalette.dataset.mediaPickerInputValue);
            updateMediaPickerDisplays(document);
        }
    }
});

document.addEventListener('DOMContentLoaded', function() {
    updateMediaSourceFields(document);
    ensureContentOrderingScript(document);
    updateLinkEditorDisplays(document);
    updateMediaPickerDisplays(document);
});

document.addEventListener('click', function(event) {
    const mediaTrigger = event.target.closest('[data-media-picker-trigger="true"]');
    if (mediaTrigger) {
        openMediaPicker(mediaTrigger);
        return;
    }

    const trigger = event.target.closest('[data-link-editor-trigger="true"]');
    if (trigger) {
        openLinkEditor(trigger);
        return;
    }

    const mediaCloseButton = event.target.closest('[data-media-picker-close="true"]');
    if (mediaCloseButton) {
        closeMediaPicker();
        return;
    }

    const closeButton = event.target.closest('[data-link-editor-close="true"]');
    if (closeButton) {
        closeLinkEditor();
        return;
    }

    const mediaSaveButton = event.target.closest('[data-media-picker-save="true"]');
    if (mediaSaveButton) {
        saveMediaPickerSelection();
        return;
    }

    const saveButton = event.target.closest('[data-link-editor-save="true"]');
    if (saveButton) {
        saveLinkEditorSelection();
        return;
    }

    const clearButton = event.target.closest('[data-link-editor-clear="true"]');
    if (clearButton) {
        clearLinkEditorSelectionValue();
        return;
    }

    const item = event.target.closest('[data-link-editor-item="true"]');
    if (item) {
        selectLinkEditorItem(item);
        const customUrlInput = document.getElementById('linkEditorCustomUrl');
        if (customUrlInput) customUrlInput.value = '';
    }

    const mediaItem = event.target.closest('[data-media-picker-item="true"]');
    if (mediaItem) {
        selectMediaPickerItem(mediaItem);
        const customUrlInput = document.getElementById('mediaPickerCustomUrl');
        if (customUrlInput) customUrlInput.value = '';
    }
});

document.addEventListener('input', function(event) {
    const target = event.target;
    if (target && target.id === 'linkEditorCustomUrl') {
        const palette = getLinkEditorPalette();
        clearLinkEditorSelection(palette);
    }

    if (target && target.id === 'mediaPickerCustomUrl') {
        const palette = getMediaPickerPalette();
        clearMediaPickerSelection(palette);
    }
});
