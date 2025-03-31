function onPasswordChange() {
    var inputPassword = document.getElementById('password');
    var inputOldPassword = document.getElementById('OldPassword');
    var informList = document.getElementById('informLabel');

    if (!inputPassword.value) {
        toastr.warning('Password cannot be empty', 'Warning');
    }
    else {
        fetchWithAuth('/api/User/password-update', {
            method: 'PATCH',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                UserId: getUserid(),
                NewPassword: inputPassword.value,
                OldPassword: inputOldPassword.value
            })
        })
            .then((response) => {
                if (response.ok) {
                    toastr.success(
                        'Password changed',
                        'Success',
                        {
                            timeOut: 2000,
                            fadeOut: 1000,
                            onHidden: function () {
                                window.location.href = "index.html";
                            }
                        }
                    )
                }
                else {
                    toastr.error('Password change failed', 'Error');
                    response.json().then(data => {
                        informList.innerHTML = '';
                        if (Array.isArray(data)) {
                            data.forEach(message => {
                                var listItem = document.createElement('li');
                                listItem.innerText = message;
                                informList.appendChild(listItem);
                            });
                        } else {
                            var listItem = document.createElement('li');
                            listItem.innerText = data;
                            informList.appendChild(listItem);
                        }
                        informList.classList.remove('hidden');
                    });
                }
            })
            .catch((error) => {
                alert(error);
            });
    }
}

function createChangePasswordForm() {
    /* Title. */
    var mainTitle = document.createElement('h1');
    mainTitle.innerText = 'Change password';

    var main = document.getElementById('main');
    main.innerHTML = '';
    main.appendChild(mainTitle);

    /* Password. */
    var labelPassword = document.createElement('label');
    labelPassword.innerText = 'New password';

    var inputPassword = document.createElement('input');
    inputPassword.id = 'password';
    inputPassword.type = 'password';

    var divPassword = document.createElement('div');
    divPassword.appendChild(labelPassword);
    divPassword.innerHTML += '<br>';
    divPassword.appendChild(inputPassword);

    /* Old Password. */
    var labelConfirmPassword = document.createElement('label');
    labelConfirmPassword.innerText = 'Old password';

    var inputOldPassword = document.createElement('input');
    inputOldPassword.id = 'OldPassword';
    inputOldPassword.type = 'password';

    var divOldPassword = document.createElement('div');
    divOldPassword.innerHTML += '<br>';
    divOldPassword.appendChild(labelConfirmPassword);
    divOldPassword.innerHTML += '<br>';
    divOldPassword.appendChild(inputOldPassword);

    /* Change button. */
    var submitButton = document.createElement('input');
    submitButton.type = 'submit';
    submitButton.value = 'Change';

    var divButton = document.createElement('div');
    divButton.innerHTML += '<br>';
    divButton.appendChild(submitButton);

    /* Login form. */
    var loginForm = document.createElement('form');
    loginForm.action = 'javascript:onPasswordChange()';
    loginForm.appendChild(divPassword);
    loginForm.appendChild(divOldPassword);
    loginForm.appendChild(divButton);

    informLabel = document.createElement('ul');
    informLabel.id = 'informLabel';
    informLabel.classList.add('warning');
    informLabel.classList.add('hidden');

    loginForm.appendChild(informLabel);

    main.appendChild(loginForm);
}

function fetchWithAuth(url, options = {}) {
    const token = localStorage.getItem('authToken');
    if (!options.headers) {
        options.headers = {};
    }
    options.headers['Authorization'] = `Bearer ${token}`;
    return fetch(url, options);
}

function getUserid() {
    const token = localStorage.getItem('authToken');
    if (!token) return null;
    const parsedToken = parseJwt(token);
    return parsedToken.nameid;
}

function parseJwt(token) {
    var base64Url = token.split('.')[1];
    var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    var jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function (c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));

    return JSON.parse(jsonPayload);
}

