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

document.addEventListener('change', function(event) {
    const target = event.target;
    if (target.matches('[data-media-source-type-select="true"]')) {
        updateMediaSourceFields(target.closest('form') || document);
    }
});

document.addEventListener('htmx:afterSwap', function(event) {
    updateMediaSourceFields(event.detail.target || document);
    ensureContentOrderingScript(event.detail.target || document);
});

document.addEventListener('DOMContentLoaded', function() {
    updateMediaSourceFields(document);
    ensureContentOrderingScript(document);
});
