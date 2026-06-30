/**
 * UCMS Swagger UI — Custom Auth Integration
 * Authorize tugmasi bosilganda login sahifasini ochadi
 * va token avtomatik Swagger'ga qo'llanadi.
 * 401 holati → login sahifasiga yo'naltiradi.
 * Logout → tokenni o'chiradi, Authorize tugmasi qayta ko'rinadi.
 */
(function () {
  'use strict';

  var TOKEN_KEY  = 'ucms_access_token';
  var LOGIN_PATH = '/auth/login';

  // ─── JWT yordamchi ────────────────────────────────────────────────────────
  function isTokenExpired(token) {
    try {
      var payload = JSON.parse(atob(token.split('.')[1]));
      return payload.exp && payload.exp * 1000 < Date.now();
    } catch (e) {
      return true;
    }
  }

  // ─── Muddati o'tgan tokenni DARHOL o'chirish (sahifa yuklanishi bilan) ────
  // Bu redirect qilmaydi — faqat localStorage'ni tozalaydi,
  // shunda Authorize popup'i ochildiganda login sahifasi eski tokenni ko'rmaydi.
  (function clearExpiredTokenNow() {
    var token = localStorage.getItem(TOKEN_KEY);
    if (token && isTokenExpired(token)) {
      localStorage.removeItem(TOKEN_KEY);
    }
  })();

  // ─── Swagger UI'ga token qo'llash ─────────────────────────────────────────
  function applyStoredToken() {
    var token = localStorage.getItem(TOKEN_KEY);
    if (token && window.ui) {
      window.ui.preauthorizeApiKey('Bearer', token);
    }
    updateLogoutButton();
  }

  // ─── Logout tugmasini ko'rsatish / yashirish ──────────────────────────────
  function updateLogoutButton() {
    var btn = document.getElementById('ucms-logout-btn');
    if (!btn) return;
    btn.style.display = localStorage.getItem(TOKEN_KEY) ? 'block' : 'none';
  }

  // ─── Logout: tokenni o'chiradi, Swagger'dan ham tozalaydi ─────────────────
  // Sahifadan chiqarmaydi — foydalanuvchi qayta Authorize orqali kirishim mumkin.
  // 401 xatoligi bo'lganda esa login sahifasiga yo'naltiradi (pastda interceptor).
  function logout() {
    localStorage.removeItem(TOKEN_KEY);
    if (window.ui) { window.ui.preauthorizeApiKey('Bearer', ''); }
    updateLogoutButton();
    showToast('🚪 Token o\'chirildi. Authorize tugmasi orqali qayta kiring.');
  }

  // ─── 401 uchun login sahifasiga yo'naltirish ─────────────────────────────
  function redirectToLogin() {
    localStorage.removeItem(TOKEN_KEY);
    window.location.href = LOGIN_PATH;
  }

  // ─── fetch interceptor — 401 ushlaydi ────────────────────────────────────
  (function interceptFetch() {
    var orig = window.fetch;
    window.fetch = function () {
      var args = arguments;
      return orig.apply(this, args).then(function (response) {
        if (response.status === 401) { redirectToLogin(); }
        return response;
      });
    };
  })();

  // ─── XHR interceptor — 401 ushlaydi ──────────────────────────────────────
  (function interceptXhr() {
    var origOpen = XMLHttpRequest.prototype.open;
    XMLHttpRequest.prototype.open = function () {
      this.addEventListener('load', function () {
        if (this.status === 401) { redirectToLogin(); }
      });
      return origOpen.apply(this, arguments);
    };
  })();

  // ─── Logout tugmasini Authorize tugmasi yoniga qo'shadi ──────────────────
  function injectLogoutButton() {
    if (document.getElementById('ucms-logout-btn')) { updateLogoutButton(); return; }
    var wrapper = document.querySelector('.auth-wrapper');
    if (!wrapper) return;

    var btn = document.createElement('button');
    btn.id        = 'ucms-logout-btn';
    btn.className = 'btn ucms-logout'; // MUHIM: 'authorize' class yo'q — capture listener ushlamasin
    btn.textContent = '🚪 Logout';
    btn.style.cssText = [
      'display:none', 'margin-left:10px',
      'background:#e53e3e', 'color:#fff',
      'border:2px solid #e53e3e', 'border-radius:4px',
      'padding:6px 16px', 'font-size:14px',
      'font-weight:700', 'cursor:pointer',
    ].join(';');
    btn.addEventListener('mouseenter', function () { btn.style.background = '#c53030'; btn.style.borderColor = '#c53030'; });
    btn.addEventListener('mouseleave', function () { btn.style.background = '#e53e3e'; btn.style.borderColor = '#e53e3e'; });
    btn.addEventListener('click', function (e) {
      e.stopImmediatePropagation();
      e.preventDefault();
      logout();
    });
    wrapper.appendChild(btn);
    updateLogoutButton();
  }

  // ─── Swagger UI render bo'lishini kuzatadi ────────────────────────────────
  function watchAndInject() {
    if (document.querySelector('.auth-wrapper')) {
      injectLogoutButton();
      return;
    }
    var observer = new MutationObserver(function () {
      if (document.querySelector('.auth-wrapper')) {
        observer.disconnect();
        injectLogoutButton();
      }
    });
    observer.observe(document.body, { childList: true, subtree: true });
  }

  // ─── Login sahifasidan postMessage orqali token qabul qiladi ─────────────
  window.addEventListener('message', function (e) {
    if (e.data && e.data.ucmsToken) {
      var token = e.data.ucmsToken;
      localStorage.setItem(TOKEN_KEY, token);
      if (window.ui) { window.ui.preauthorizeApiKey('Bearer', token); }
      injectLogoutButton();
      showToast('✅ Token muvaffaqiyatli qo\'yildi!');
    }
  });

  // ─── Authorize tugmasini ushlab qoladi (capture phase) ───────────────────
  // MUHIM: faqat Swagger'ning o'z Authorize tugmasi — logout tugmasi emas
  document.addEventListener('click', function (e) {
    var authorizeBtn = e.target.closest('.btn.authorize');
    if (!authorizeBtn) return;
    // Logout tugmasimiz 'btn ucms-logout' class'ga ega — bu shart uni filtrdan o'tkazmaydi.
    // Lekin agar kimdir authorize class bilan custom tugma qo'shsa deb tekshiramiz:
    if (authorizeBtn.id === 'ucms-logout-btn') return;

    e.stopImmediatePropagation();

    // Popup ochishdan oldin expired token ni tozalash (1200ms kechikish yo'q deb)
    var existing = localStorage.getItem(TOKEN_KEY);
    if (existing && isTokenExpired(existing)) {
      localStorage.removeItem(TOKEN_KEY);
    }

    var w    = 500, h = 640;
    var left = Math.round((screen.width  - w) / 2);
    var top  = Math.round((screen.height - h) / 2);
    var popup = window.open(
      LOGIN_PATH,
      'ucms-login',
      'width=' + w + ',height=' + h + ',left=' + left + ',top=' + top + ',resizable=yes'
    );

    if (!popup) {
      window.open(LOGIN_PATH, '_blank');
      showToast('ℹ️ Login sahifasi yangi tabda ochildi.');
      return;
    }

    // Token kelishini kutish (postMessage yoki polling)
    var poll = setInterval(function () {
      var token = localStorage.getItem(TOKEN_KEY);
      if (token && !isTokenExpired(token) && window.ui) {
        clearInterval(poll);
        window.ui.preauthorizeApiKey('Bearer', token);
        injectLogoutButton();
        showToast('✅ Token qo\'yildi! Endi API\'larni test qilishingiz mumkin.');
        if (popup && !popup.closed) { popup.close(); }
      }
      if (popup.closed) { clearInterval(poll); }
    }, 600);
  }, true /* capture phase */);

  // ─── Toast ────────────────────────────────────────────────────────────────
  function showToast(msg) {
    var toast = document.getElementById('ucms-toast');
    if (!toast) {
      toast = document.createElement('div');
      toast.id = 'ucms-toast';
      toast.style.cssText = [
        'position:fixed', 'bottom:24px', 'right:24px', 'z-index:99999',
        'background:#1a1a2e', 'color:#fff', 'padding:12px 20px',
        'border-radius:8px', 'font-size:14px', 'font-weight:600',
        'box-shadow:0 4px 20px rgba(0,0,0,.4)', 'transition:opacity .3s',
      ].join(';');
      document.body.appendChild(toast);
    }
    toast.textContent = msg;
    toast.style.opacity = '1';
    clearTimeout(toast._hide);
    toast._hide = setTimeout(function () { toast.style.opacity = '0'; }, 3500);
  }

  // ─── Sahifa yuklanishi ────────────────────────────────────────────────────
  window.addEventListener('load', function () {
    watchAndInject();
    setTimeout(applyStoredToken, 1200);
  });
})();
