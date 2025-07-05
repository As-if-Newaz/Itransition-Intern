document.addEventListener('DOMContentLoaded', function() {
    const themeButton = document.getElementById('btnTheme');
    const lightIcon = document.getElementById('light-icon');
    const darkIcon = document.getElementById('dark-icon');
    
    // Initialize theme on page load
    initializeTheme();
    
    // Add click event listener
    if (themeButton) {
        themeButton.addEventListener('click', toggleTheme);
    }
    
    function initializeTheme() {
        const currentTheme = getCookie('Theme') || 'Light';
        applyTheme(currentTheme);
        updateButtonIcon(currentTheme);
    }
    
    function toggleTheme() {
        const currentTheme = getCookie('Theme') || 'Light';
        const newTheme = currentTheme === 'Dark' ? 'Light' : 'Dark';
        
        // Set cookie for immediate effect
        document.cookie = `Theme=${newTheme}; path=/; max-age=${365 * 24 * 60 * 60}; SameSite=Lax`;
        
        // Apply theme immediately
        applyTheme(newTheme);
        updateButtonIcon(newTheme);
        
        // If user is logged in, save preference to database via AJAX
        const loggedId = getCookie('LoggedId');
        if (loggedId) {
            saveThemePreference(loggedId, newTheme);
        }
    }
    
    function saveThemePreference(userId, theme) {
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
        fetch('/User/UpdateThemePreference', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({
                userId: userId,
                theme: theme
            })
        })
        .then(response => response.json())
        .then(data => {
            if (!data.success) {
                console.warn('Failed to save theme preference to database');
            }
        })
        .catch(error => {
            console.error('Error saving theme preference:', error);
        });
    }
    
    function applyTheme(theme) {
        const html = document.documentElement;
        
        if (theme === 'Dark') {
            html.setAttribute('data-bs-theme', 'dark');
            document.body.classList.add('dark-theme');
        } else {
            html.setAttribute('data-bs-theme', 'light');
            document.body.classList.remove('dark-theme');
        }
    }
    
    function updateButtonIcon(theme) {
        if (theme === 'Dark') {
            lightIcon.style.display = 'inline-block';
            darkIcon.style.display = 'none';
        } else {
            lightIcon.style.display = 'none';
            darkIcon.style.display = 'inline-block';
        }
    }
    
    function getCookie(name) {
        const value = `; ${document.cookie}`;
        const parts = value.split(`; ${name}=`);
        if (parts.length === 2) return parts.pop().split(';').shift();
        return null;
    }
}); 