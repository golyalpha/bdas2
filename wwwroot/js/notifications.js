// Notifications handling
(function() {
    'use strict';

    let notificationCheckInterval;
    const CHECK_INTERVAL = 30000; // 30 sekund
    let currentNotificationIds = []; // ID notifikací zobrazených v dropdownu

    function getAntiForgeryToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        if (tokenInput) {
            return tokenInput.value;
        }
        
        const cookies = document.cookie.split(';');
        for (let cookie of cookies) {
            const trimmed = cookie.trim();
            if (trimmed.startsWith('__RequestVerificationToken=')) {
                return trimmed.substring('__RequestVerificationToken='.length);
            }
        }
        
        return null;
    }

    // Lehká funkce pouze pro aktualizaci badge (volá PL/SQL funkci)
    function updateNotificationBadge() {
        const badge = document.getElementById('notificationBadge');
        if (!badge) {
            console.warn('Notification badge element not found');
            return;
        }

        fetch('/Notification/GetUnreadCount')
            .then(response => {
                if (response.status === 401) {
                    // Uživatel není pøihlášený - skryjeme badge
                    badge.style.display = 'none';
                    return null;
                }
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                if (data === null) return; // Uživatel není pøihlášený

                // Zobrazit badge pouze pokud count > 0
                if (data.count > 0) {
                    badge.textContent = data.count;
                    badge.style.display = 'inline-block';
                    console.log(`Notification badge updated: ${data.count} unread`);
                } else {
                    badge.style.display = 'none';
                    console.log('No unread notifications');
                }
            })
            .catch(error => {
                console.error('Error loading notification count:', error);
                // V pøípadì chyby schováme badge
                badge.style.display = 'none';
            });
    }

    // Plné naètení notifikací (volá se jen pøi otevøení dropdownu)
    function loadNotifications() {
        fetch('/Notification/GetForCurrentUser')
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(data => {
                const undelivered = data.filter(n => !n.delivered);
                // Uložíme ID všech nepøeètených notifikací
                currentNotificationIds = undelivered.map(n => n.id);
                updateNotificationUI(undelivered);
            })
            .catch(error => {
                console.error('Error loading notifications:', error);
                // Zobrazíme chybovou hlášku v dropdownu
                const listContainer = document.getElementById('notificationList');
                if (listContainer) {
                    listContainer.innerHTML = '<li class="dropdown-item text-center text-danger">Chyba pøi naèítání notifikací</li>';
                }
            });
    }

    function updateNotificationUI(notifications) {
        const badge = document.getElementById('notificationBadge');
        const listContainer = document.getElementById('notificationList');

        if (!badge || !listContainer) return;

        // Update badge - zobrazit pouze pokud count > 0
        if (notifications.length > 0) {
            badge.textContent = notifications.length;
            badge.style.display = 'inline-block';
        } else {
            badge.style.display = 'none';
        }

        // Update list
        if (notifications.length === 0) {
            listContainer.innerHTML = '<li class="dropdown-item text-center text-muted">Žádné nové notifikace</li>';
        } else {
            // OPRAVA: Odkaz vede na /Notification/Index, NE na detail žádosti
            // NEOZNAÈUJEME notifikace pøi kliknutí
            listContainer.innerHTML = notifications.map(n => `
                <li>
                    <a class="dropdown-item notification-item" href="/Notification/Index">
                        <div>
                            <strong>${getNotificationTypeText(n.notificationType)}</strong>
                            <div class="small text-muted">${n.locationName || 'Neznámá lokace'}</div>
                            <div class="small">${formatDateTime(n.requestStart)}</div>
                        </div>
                    </a>
                </li>
            `).join('');

            // ŽÁDNÉ event listenery pro kliknutí - prostì pøesmìrujeme na seznam
        }
    }

    function markAsDelivered(notificationIds) {
        if (!notificationIds || notificationIds.length === 0) return;

        const token = getAntiForgeryToken();
        if (!token) {
            console.error('Antiforgery token not found');
            return;
        }

        console.log(`Marking ${notificationIds.length} notifications as delivered:`, notificationIds);

        fetch('/Notification/MarkDelivered', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(notificationIds)
        })
        .then(response => {
            if (response.ok) {
                console.log(`Successfully marked ${notificationIds.length} notification(s) as delivered`);
                // Aktualizujeme badge
                updateNotificationBadge();
            } else {
                console.error('Failed to mark notifications as delivered');
            }
        })
        .catch(error => {
            console.error('Error marking notifications as delivered:', error);
        });
    }

    function getNotificationTypeText(type) {
        const types = {
            'ALLOC_SUCCESS': 'Místnost pøidìlena',
            'ALLOC_FAIL': 'Pøidìlení selhalo',
            'APPROVED': 'Schváleno',
            'REJECTED': 'Zamítnuto',
            'PENDING': 'Èeká na schválení',
            'CANCELLED': 'Zrušeno'
        };
        return types[type] || type;
    }

    function formatDateTime(dateString) {
        if (!dateString) return '';
        const date = new Date(dateString);
        return date.toLocaleDateString('cs-CZ', { 
            day: '2-digit', 
            month: '2-digit', 
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    // Initialize
    document.addEventListener('DOMContentLoaded', function() {
        console.log('Notification system initializing...');
        
        const notificationDropdown = document.getElementById('notificationDropdown');
        if (notificationDropdown) {
            console.log('Notification dropdown found');
            
            // Pøi otevøení dropdownu naèteme plné notifikace
            notificationDropdown.addEventListener('show.bs.dropdown', function() {
                console.log('Opening notification dropdown');
                loadNotifications();
            });

            // KLÍÈOVÁ OPRAVA: Pøi zavøení dropdownu oznaèíme VŠECHNY zobrazené notifikace jako delivered
            notificationDropdown.addEventListener('hide.bs.dropdown', function() {
                console.log('Closing notification dropdown');
                if (currentNotificationIds.length > 0) {
                    console.log(`Marking ${currentNotificationIds.length} notifications as delivered`);
                    markAsDelivered(currentNotificationIds);
                    currentNotificationIds = []; // Vyèistíme seznam
                }
            });
            
            // Initial badge load
            console.log('Loading initial notification count...');
            updateNotificationBadge();
            
            // Pravidelná aktualizace pouze poètu
            notificationCheckInterval = setInterval(function() {
                console.log('Periodic notification count refresh');
                updateNotificationBadge();
            }, CHECK_INTERVAL);
            
            console.log(`Notification system initialized. Refresh interval: ${CHECK_INTERVAL}ms`);
        } else {
            console.warn('Notification dropdown not found - user probably not logged in');
        }
    });

    // Cleanup
    window.addEventListener('beforeunload', function() {
        if (notificationCheckInterval) {
            clearInterval(notificationCheckInterval);
            console.log('Notification check interval cleared');
        }
    });
})();