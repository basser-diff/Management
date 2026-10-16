// ── RESTO Notification helpers ───────────────────────────────────────────────

// Play a notification chime using Web Audio API
window.restoPlayChime = function (type) {
    try {
        const ctx = new (window.AudioContext || window.webkitAudioContext)();
        const frequencies = {
            'Chime': [880, 1100, 1320],
            'Bell':  [523, 659, 784],
            'Ping':  [1047, 0, 0],
            'Ding':  [784, 988, 0],
            'Alert': [440, 554, 659],
        };
        const notes = frequencies[type] || frequencies['Chime'];
        let time = ctx.currentTime;
        notes.filter(f => f > 0).forEach(freq => {
            const osc  = ctx.createOscillator();
            const gain = ctx.createGain();
            osc.connect(gain);
            gain.connect(ctx.destination);
            osc.frequency.value = freq;
            osc.type = 'sine';
            gain.gain.setValueAtTime(0.25, time);
            gain.gain.exponentialRampToValueAtTime(0.001, time + 0.4);
            osc.start(time);
            osc.stop(time + 0.4);
            time += 0.18;
        });
    } catch (e) {
        console.warn('Audio not available:', e);
    }
};

// Show a browser toast notification
window.restoShowToast = function (title, message, type) {
    // Remove old toasts
    document.querySelectorAll('.resto-toast').forEach(t => t.remove());

    const colors = {
        'NewOrder':         '#1565c0',
        'PaymentConfirmed': '#2e7d32',
        'LowStock':         '#f57f17',
        'OrderReady':       '#00695c',
        'General':          '#6a1b9a',
    };
    const icons = {
        'NewOrder':         '🛍️',
        'PaymentConfirmed': '💳',
        'LowStock':         '⚠️',
        'OrderReady':       '✅',
        'General':          '🔔',
    };

    const color = colors[type] || '#b71c1c';
    const icon  = icons[type]  || '🔔';

    const toast = document.createElement('div');
    toast.className = 'resto-toast';
    toast.innerHTML = `
        <div style="display:flex;align-items:center;gap:10px;">
            <span style="font-size:22px;">${icon}</span>
            <div>
                <div style="font-weight:700;font-size:13px;color:#111;">${title}</div>
                <div style="font-size:12px;color:#555;margin-top:2px;">${message}</div>
            </div>
            <button onclick="this.closest('.resto-toast').remove()"
                    style="margin-left:auto;background:none;border:none;cursor:pointer;font-size:16px;color:#888;padding:0 4px;">✕</button>
        </div>
        <div class="resto-toast-bar" style="background:${color};"></div>
    `;

    Object.assign(toast.style, {
        position:     'fixed',
        bottom:       '90px',
        right:        '24px',
        background:   'white',
        borderRadius: '12px',
        boxShadow:    '0 6px 24px rgba(0,0,0,0.15)',
        padding:      '14px 16px 6px',
        minWidth:     '300px',
        maxWidth:     '380px',
        zIndex:       '99999',
        animation:    'restoSlideIn 0.3s ease-out',
        overflow:     'hidden',
        borderLeft:   `4px solid ${color}`,
    });

    document.body.appendChild(toast);

    // Progress bar animation
    const bar = toast.querySelector('.resto-toast-bar');
    Object.assign(bar.style, {
        height:   '3px',
        marginTop:'8px',
        borderRadius:'2px',
        transition:'width 5s linear',
        width:    '100%',
    });
    requestAnimationFrame(() => {
        requestAnimationFrame(() => { bar.style.width = '0%'; });
    });

    // Auto-dismiss after 5s
    setTimeout(() => {
        toast.style.animation = 'restoSlideOut 0.3s ease-in';
        setTimeout(() => toast.remove(), 300);
    }, 5000);
};

// Inject keyframe animations once
if (!document.getElementById('resto-toast-style')) {
    const style = document.createElement('style');
    style.id = 'resto-toast-style';
    style.textContent = `
        @keyframes restoSlideIn  { from { opacity:0; transform:translateY(20px); } to { opacity:1; transform:translateY(0); } }
        @keyframes restoSlideOut { from { opacity:1; transform:translateY(0);    } to { opacity:0; transform:translateY(20px); } }
    `;
    document.head.appendChild(style);
}
