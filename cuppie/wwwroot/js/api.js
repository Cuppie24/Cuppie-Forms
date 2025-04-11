window.sendApiRequest = async (url, method = 'GET', body = null) => {
    const options = {
        method: method,
        credentials: 'include', // обязательно для работы с куками
        headers: {
            'Content-Type': 'application/json'
        }
    };

    if (body) {
        options.body = JSON.stringify(body);
    }

    const response = await fetch(url, options);
    const contentType = response.headers.get("content-type");
    let responseBody;

    if (contentType && contentType.includes("application/json")) {
        responseBody = await response.json();
    } else {
        responseBody = await response.text();
    }

    return {
        status: response.status,
        ok: response.ok,
        body: responseBody,
        headers: Object.fromEntries(response.headers.entries())
    };
};
