window.jadwalSholatInterop = {
    scrollCurrentPrayerIntoView: function () {
        var el = document.querySelector('.prayer-grid-cell.is-current');
        if (el) el.scrollIntoView({ behavior: 'smooth', inline: 'center', block: 'nearest' });
    },
    getCurrentPosition: function () {
        return new Promise((resolve, reject) => {
            if (!navigator.geolocation) {
                reject("Geolocation tidak didukung oleh browser ini.");
                return;
            }
            navigator.geolocation.getCurrentPosition(
                pos => resolve({ latitude: pos.coords.latitude, longitude: pos.coords.longitude }),
                err => reject(err.message || "Gagal mendapatkan lokasi."),
                { enableHighAccuracy: false, timeout: 10000, maximumAge: 600000 }
            );
        });
    }
};
