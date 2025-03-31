var tokenKey = 'authToken';

function onLoginStep1() {

    event.preventDefault(); // Verhindert das Standard-Formular-Submit-Verhalten

    var inputUsername = document.getElementById("username");
    var inputPassword = document.getElementById("password");

    fetch("/api/Login/login-step1", {
        method: "POST",
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ Username: inputUsername.value, Password: inputPassword.value })
    })
        .then((response) => {
            if (response.ok) {
                return response.json();
            } else {
                throw new Error("Login step 1 failed: " + response.statusText + " (" + response.status + ")");
            }
        })
        .then((data) => {
            if (data.qrCodeUrl) {
                console.log("QR Code URL:", data.qrCodeUrl); // Debugging line
                console.log("Suche nach #qrCode");
                var qrCodeImage = document.getElementById('qrCode');
                console.log("qrCodeImage:", qrCodeImage);

                var qrCodeImage = document.getElementById('qrCode');
                qrCodeImage.src = data.qrCodeUrl;
                qrCodeImage.style.display = 'block'; // Ensure the image is displayed
                document.getElementById('twoFactorSection').classList.remove('hidden');
            } else {
                throw new Error("QR code URL not found in response");
            }
        })
        .catch((error) => {
            var labelResult = document.getElementById("labelResult");
            labelResult.innerText = error.message;
            labelResult.classList.remove("hidden");
        });
}



function onLoginStep2() {

    event.preventDefault(); // Verhindert das Standard-Formular-Submit-Verhalten

    var inputUsername = document.getElementById("username");
    var inputTwoFactorToken = document.getElementById("twoFactorToken");

    fetch("/api/Login/login-step2", {
        method: "POST",
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ Username: inputUsername.value, TwoFactorToken: inputTwoFactorToken.value })
    })
        .then((response) => {
            if (response.ok) {
                return response.json();
            } else {
                throw new Error("Login step 2 failed: " + response.statusText + " (" + response.status + ")");
            }
        })
        .then((data) => {
            if (data.token) {
                saveToken(data.token);
                window.location.href = "index.html";
            } else {
                throw new Error("Token not found in response");
            }
        })
        .catch((error) => {
            var labelResult = document.getElementById("labelResult");
            labelResult.innerText = error.message;
            labelResult.classList.remove("hidden");
        });
}

function saveToken(token) {
    localStorage.setItem(tokenKey, token);
    // Entfernen von loggedInUser aus dem localStorage
    localStorage.removeItem('loggedInUser');
}

function getToken() {
    return localStorage.getItem(tokenKey);
}

function fetchWithAuth(url, options = {}) {
    const token = getToken();
    if (!options.headers) {
        options.headers = {};
    }
    options.headers['Authorization'] = `Bearer ${token}`;
    return fetch(url, options);
}

function toggleDropdown() {
    var dropdownContent = document.getElementById("dropdownContent");
    dropdownContent.style.display = dropdownContent.style.display === "block" ? "none" : "block";
}

function logout() {
    var dropdownContent = document.getElementById("dropdownContent");
    dropdownContent.style.display = dropdownContent.style.display === "block" ? "none" : "block";
    resetToken();
    window.location.href = "index.html";
}

function resetToken() {
    localStorage.removeItem(tokenKey);
}

function getUsername() {
    const token = getToken();
    if (!token) return null;
    const parsedToken = parseJwt(token);
    return parsedToken.unique_name;
}

function getUserid() {
    const token = getToken();
    if (!token) return null;
    const parsedToken = parseJwt(token);
    return parsedToken.nameid;
}

function isAdmin() {
    const token = getToken();
    if (!token) return false;
    const parsedToken = parseJwt(token);
    return parsedToken.role === "admin";
}

function isLoggedIn() {
    return getToken() != null;
}

function createLoginForm() {
    /* Title. */
    var mainTitle = document.createElement("h1");
    mainTitle.innerText = "Login";

    var main = document.getElementById("main");
    main.appendChild(mainTitle);

    /* Username. */
    var labelUsername = document.createElement("label");
    labelUsername.innerText = "Username";

    var inputUsername = document.createElement("input");
    inputUsername.id = "username";

    var divUsername = document.createElement("div");
    divUsername.appendChild(labelUsername);
    divUsername.innerHTML += '<br>';
    divUsername.appendChild(inputUsername);

    /* Password. */
    var labelPassword = document.createElement("label");
    labelPassword.innerText = "Password";

    var inputPassword = document.createElement("input");
    inputPassword.id = "password";
    inputPassword.type = "password";

    var divPassword = document.createElement("div");
    divPassword.innerHTML += '<br>';
    divPassword.appendChild(labelPassword);
    divPassword.innerHTML += '<br>';
    divPassword.appendChild(inputPassword);

    /* Result label */
    var labelResult = document.createElement("label");
    labelResult.innerText = "Login result";
    labelResult.id = "labelResult";
    labelResult.classList.add("warning");
    labelResult.classList.add("hidden");

    var divResult = document.createElement("div");
    divResult.appendChild(labelResult);

    /* Login button. */
    var submitButton = document.createElement("input");
    submitButton.type = "submit";
    submitButton.value = "Login";
    submitButton.onclick = onLoginStep1;

    var divButton = document.createElement("div");
    divButton.appendChild(submitButton);

    /* Login form. */
    var loginForm = document.createElement("form");
    loginForm.appendChild(divUsername);
    loginForm.appendChild(divPassword);
    loginForm.appendChild(divResult);
    loginForm.appendChild(divButton);

    main.appendChild(loginForm);

    /* Two-Factor Section */
    var twoFactorSection = document.createElement("div");
    twoFactorSection.id = "twoFactorSection";
    twoFactorSection.classList.add("hidden");

    var labelTwoFactorToken = document.createElement("label");
    labelTwoFactorToken.innerText = "2FA Token";

    var inputTwoFactorToken = document.createElement("input");
    inputTwoFactorToken.id = "twoFactorToken";

    var divTwoFactorToken = document.createElement("div");
    divTwoFactorToken.innerHTML += '<br>';
    divTwoFactorToken.appendChild(labelTwoFactorToken);
    divTwoFactorToken.innerHTML += '<br>';
    divTwoFactorToken.appendChild(inputTwoFactorToken);

    var qrCodeImage = document.createElement("img");
    qrCodeImage.id = "qrCode";

    var submitButton2 = document.createElement("input");
    submitButton2.type = "submit";
    submitButton2.value = "Verify 2FA";
    submitButton2.onclick = onLoginStep2;

    var divButton2 = document.createElement("div");
    divButton2.appendChild(submitButton2);

    twoFactorSection.appendChild(divTwoFactorToken);
    twoFactorSection.appendChild(qrCodeImage);
    twoFactorSection.appendChild(divButton2);

    main.appendChild(twoFactorSection);
}

function parseJwt(token) {
    var base64Url = token.split('.')[1];
    var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    var jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function (c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));

    return JSON.parse(jsonPayload);
}

