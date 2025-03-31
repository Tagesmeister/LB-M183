function handleCancelNew(newsEntry) {
    window.location.href = "index.html";
}

function handleSaveNew() {
    var headerElement = document.getElementById("header");
    var detailElement = document.getElementById("detail");

    var data = {
        header: headerElement.innerText,
        detail: detailElement.innerText,
        authorId: getUserid(),
        isAdminNews: isAdmin()
    }

    fetchWithAuth("/api/News/", {
        method: "POST",
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
    })
        .then((response) => {
            if (response.ok) {
                window.location.href = "index.html";
            }
            else {
                alert("NOK");
            }
        })
        .catch(() => {
            alert("Error");
        });
}

function createNews(data) {
    /* Header. */
    var mainTitle = document.createElement("h1");
    mainTitle.innerText = "New entry";

    var main = document.getElementById("main");
    main.appendChild(mainTitle);

    /* New entry. */
    var divEntry = document.createElement("div");
    divEntry.classList.add("newsEntry");

    var header = document.createElement("div");
    header.id = "header";
    header.classList.add("newsHeader");
    if (isAdmin()) {
        header.classList.add("adminNews");
    }
    header.innerText = "New header";
    header.contentEditable = true;

    var detail = document.createElement("div");
    detail.id = "detail";
    detail.classList.add("newsDetail");
    detail.innerText = "New detail";
    detail.contentEditable = true;

    var btnSave = document.createElement("button");
    btnSave.id = "btnSave";
    btnSave.classList.add("btnSave");
    btnSave.innerText = "Save";

    (function (arg) {
        btnSave.addEventListener("click", function () {
            handleSaveNew(arg);
        });
    })();

    var btnCancel = document.createElement("button");
    btnCancel.id = "btnCancel";
    btnCancel.classList.add("btnCancel");
    btnCancel.innerText = "Cancel";

    (function (arg) {
        btnCancel.addEventListener("click", function () {
            handleCancelNew(arg);
        });
    })();

    divEntry.appendChild(header);
    divEntry.appendChild(detail);
    divEntry.appendChild(btnSave);
    divEntry.appendChild(btnCancel);
    main.appendChild(divEntry);
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

function isAdmin() {
    const token = localStorage.getItem('authToken');
    if (!token) return false;
    const parsedToken = parseJwt(token);
    return parsedToken.role === "admin";
}

function parseJwt(token) {
    var base64Url = token.split('.')[1];
    var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    var jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function (c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));

    return JSON.parse(jsonPayload);
}

