function login() {
    fetch("/login", { method: 'post' });
}

function helloWorld() {
    fetch("/");
}

function test() {
    fetch("/test");
}

function challenge() {
    fetch("/challenge");
}

function logout() {
    fetch("/logout");
}

(() => {
    var app = document.getElementById("app");

    var loginButton = document.createElement("button");
    loginButton.innerText = "Login";
    loginButton.onclick = login;

    app.appendChild(loginButton);

    var helloButton = document.createElement("button");
    helloButton.innerText = "Hello world";
    helloButton.onclick = helloButton;

    app.appendChild(helloButton);

    var testButton = document.createElement("button");
    testButton.innerText = "Test";
    testButton.onclick = test;

    app.appendChild(testButton);

    var challengeButton = document.createElement("button");
    challengeButton.innerText = "Challenge";
    challengeButton.onclick = challenge;

    app.appendChild(challengeButton);

    var logoutButton = document.createElement("button");
    logoutButton.innerText = "Logout";
    logoutButton.onclick = logout;

    app.appendChild(logoutButton);
})();
