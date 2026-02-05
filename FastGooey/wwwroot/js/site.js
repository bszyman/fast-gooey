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