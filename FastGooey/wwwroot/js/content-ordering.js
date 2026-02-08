(() => {
    function initializeContentItemDragAndDrop(root = document) {
        const containers = root.querySelectorAll('[data-content-order-container="true"]');

        containers.forEach(container => {
            if (container.dataset.contentOrderInit === 'true') return;
            container.dataset.contentOrderInit = 'true';

            let draggedItem = null;
            let activeDropItem = null;

            const getItems = () => Array.from(container.querySelectorAll('[data-content-item-id]'));
            const inputsContainer = container.querySelector('[data-content-order-inputs="true"]');
            const hintOutline = '1px solid #d1d5db';
            const activeOutline = '1px solid #86efac';
            const activeBackground = '#dcfce7';

            const syncInputs = () => {
                if (!inputsContainer) return;
                inputsContainer.innerHTML = '';
                getItems().forEach(item => {
                    const input = document.createElement('input');
                    input.type = 'hidden';
                    input.name = 'orderedItemIds';
                    input.value = item.dataset.contentItemId || '';
                    inputsContainer.appendChild(input);
                });
            };

            const applyHintStyle = item => {
                if (item === draggedItem) return;
                item.style.outline = hintOutline;
                item.style.outlineOffset = '2px';
            };

            const applyActiveStyle = item => {
                if (item === draggedItem) return;
                item.style.outline = activeOutline;
                item.style.outlineOffset = '2px';
                item.style.backgroundColor = activeBackground;
            };

            const clearStyles = item => {
                item.style.outline = '';
                item.style.outlineOffset = '';
                item.style.backgroundColor = '';
            };

            const showDropHints = () => {
                getItems().forEach(item => applyHintStyle(item));
            };

            const clearDropHints = () => {
                getItems().forEach(item => clearStyles(item));
                activeDropItem = null;
            };

            const setActiveDropItem = item => {
                if (activeDropItem === item) return;
                if (activeDropItem) applyHintStyle(activeDropItem);
                activeDropItem = item;
                if (activeDropItem) applyActiveStyle(activeDropItem);
            };

            container.addEventListener('dragstart', event => {
                const item = event.target.closest('[data-content-item-id]');
                if (!item) return;
                draggedItem = item;
                item.classList.add('dragging');
                showDropHints();
                event.dataTransfer.effectAllowed = 'move';
                event.dataTransfer.setData('text/plain', item.dataset.contentItemId || '');
            });

            container.addEventListener('dragend', () => {
                if (draggedItem) {
                    draggedItem.classList.remove('dragging');
                }
                draggedItem = null;
                clearDropHints();
            });

            container.addEventListener('dragover', event => {
                if (!draggedItem) return;
                event.preventDefault();
                const item = event.target.closest('[data-content-item-id]');
                setActiveDropItem(item || null);
                if (!item) {
                    const items = getItems();
                    if (items.length === 0) return;
                    const lastItem = items[items.length - 1];
                    const rect = lastItem.getBoundingClientRect();
                    if (event.clientY > rect.bottom) {
                        container.appendChild(draggedItem);
                    }
                    return;
                }

                if (item === draggedItem) return;

                const rect = item.getBoundingClientRect();
                const insertAfter = (event.clientY - rect.top) > (rect.height / 2);
                const referenceNode = insertAfter ? item.nextElementSibling : item;

                if (referenceNode === draggedItem) return;
                container.insertBefore(draggedItem, referenceNode);
            });

            container.addEventListener('dragleave', event => {
                if (!draggedItem) return;
                const related = event.relatedTarget;
                const leavingContainer = !related || !container.contains(related);
                if (leavingContainer) {
                    setActiveDropItem(null);
                }
            });

            container.addEventListener('drop', event => {
                if (!draggedItem) return;
                event.preventDefault();
                syncInputs();
                clearDropHints();
                if (window.htmx) {
                    htmx.trigger(container, 'contentReordered');
                }
            });

            syncInputs();
        });
    }

    document.addEventListener('htmx:afterSwap', function(event) {
        initializeContentItemDragAndDrop(event.detail.target || document);
    });

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            initializeContentItemDragAndDrop(document);
        });
    } else {
        initializeContentItemDragAndDrop(document);
    }
})();
