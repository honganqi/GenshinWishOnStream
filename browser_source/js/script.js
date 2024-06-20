/* Genshin Impact: Wish On Stream v1.6 by honganqi */
/* https://github.com/honganqi/GenshinWishOnStream */

var scope = encodeURIComponent('channel:read:redemptions');
var ws;


var queue = [];
var queueItems = [];

var isRunning = false;
var lastItemRun = 0;

var redeemer;

let character_image_filename;
let element;

let twitchOAuthToken = "";
let twitchAuthURL = authUrl();

let cases = {};
let previousRate = 0;

rates.forEach((rate, star) => {
	if (rate > 0) {
		cases[previousRate + rate] = star;
		previousRate += rate;
	}
});


var dullBladeStar = -1;
choices.forEach(function(characters, key) {
	characters.forEach(function(character) {
		if (character.name == "Dull Blade")
			dullBladeStar = key;
	});
});


const runThis = async () => {
    for (let i = lastItemRun; i < queue.length; i++) {
        isRunning = true;
        await wish(queue[i]);
        lastItemRun++;
        isRunning = false;
    }
}

const wish = () => new Promise(function(resolve, reject) {
	genshinWish();
    setTimeout(function() {
        genshinExit();
        resolve('genshin_wish');
    }, animation_duration);

});

function addToQueue(redeem) {
    queue.push(redeem);
    if (!isRunning) {
        runThis();
    }
}


function getCharacterElement(character) {
	return elementDictionary[character];
}

function getDullBlades() {
	let choice = Math.floor(Math.random() * (dullBlades.length));
	return dullBlades[choice];
}


function genshinWish() {
	let audio = new Audio("sounds/character_appearance.ogg");

	let star = randomInt(cases);
	let choice = Math.floor(Math.random() * (choices[star].length));
	let character = choices[star][choice];
	element = 'img/elements/' + character.element;
	character_image_filename = `img/characters/${character.name}`;

	if ((dullBladeStar > 0) && (star == dullBladeStar))
		character_image_filename = 'img/characters/dull_blades/' + getDullBlades();

	// load all existing images
	var loaders = [];
	var extensions = ['webp', 'png', 'svg'];
	extensions.forEach((ext) => {
		loaders.push(imageExists(character_image_filename + '.' + ext, "character"));
	});

	extensions.forEach((ext) => {
		loaders.push(imageExists(element + '.' + ext, "element"));
	});

	// play animation once all images are checked and loaded
	$.when.apply(null, loaders).done(function() {
		setTimeout(function () {
			audio.play();
		}, 500);
		
		document.getElementById("wrapper").innerHTML = `
		<div id="container">
		<img src="${character_image_filename}" id="character">
		<h1 id="name">${character.name}</h1>
		<h2 id="redeemer"><span id="actual_name">${redeemer}</span></h2>
		<img src="${element}" id="element">
		<div id="stars"></div>
		</div>
		`;

		//writeToFile(redeemer, character);

		setTimeout(function() {
			putStars(star);
		}, 500);
	});


}

function putStars(stars) {
	let audio = [];
	for (let i = 1; i <= stars; i++) {
		audio[i] = new Audio("sounds/star.ogg");
		let star_img = document.createElement('img');
		star_img.src = "img/star.svg";

		setTimeout(function() {
			document.getElementById("stars").append(star_img);
			setTimeout(function() {
				audio[i].play();
			}, 1150);
		}, i * 150);
	}
}

function genshinExit() {
	document.getElementById("container").setAttribute("class", "exit");
}

function randomInt(cases) {
	let random = Math.floor(Math.random() * 100);
	for (let prob in cases) {
		if (prob >= random)
			return cases[prob];
	}
}

function imageExists(image_url, img_type) {
	var deferred = $.Deferred();
	/*
    var http = new XMLHttpRequest();

    http.open('HEAD', image_url, false);
    http.send();

    return http.status != 404;
    */
    var img = new Image();
    img.src = image_url;
    img.onload = () => {
    	switch (img_type) {
    		case "character":
    			character_image_filename = image_url;
    			break;
    		case "element":
    			element = image_url;
    			break;
    	}
    	deferred.resolve();
    };
    img.onerror = () => {
    	deferred.resolve();
    }
    return deferred.promise();
}

function writeToFile(user, character) {
	var xmlhttp;
	if (window.XMLHttpRequest) {
		// code for IE7+, Firefox, Chrome, Opera, Safari
		xmlhttp=new XMLHttpRequest();
	}
	else {
		// code for IE6, IE5
		xmlhttp=new ActiveXObject("Microsoft.XMLHTTP");
	}
	xmlhttp.onreadystatechange=function() {
		if (xmlhttp.readyState==4 && xmlhttp.status==200) {
			//alert('done')
		}
	}

	xmlhttp.open("POST","writer.php",true);
	xmlhttp.setRequestHeader("Content-type","application/x-www-form-urlencoded");
	xmlhttp.send(`name=${user}&character=${character.name}`);
}











// these 5 functions below are thanks to Twitch Developers over at GitHub
// https://github.com/twitchdev/pubsub-javascript-sample

// Source: https://www.thepolyglotdeveloper.com/2015/03/create-a-random-nonce-string-using-javascript/
function nonce(length) {
    var text = "";
    var possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    for (var i = 0; i < length; i++) {
        text += possible.charAt(Math.floor(Math.random() * possible.length));
    }
    return text;
}

function heartbeat() {
    message = {
        type: 'PING'
    };
    ws.send(JSON.stringify(message));
}

function listen(topics) {
    message = {
        type: 'LISTEN',
        nonce: nonce(15),
        data: {
            topics: topics,
            auth_token: twitchOAuthToken
        }
    };
    ws.send(JSON.stringify(message));
}

function connect(topics) {
    var heartbeatInterval = 1000 * 60; //ms between PING's
    var reconnectInterval = 1000 * 3; //ms to wait before reconnect
    var heartbeatHandle;

    ws = new WebSocket('wss://pubsub-edge.twitch.tv');

    ws.onopen = function(event) {
        listen(topics);
        heartbeat();
        heartbeatHandle = setInterval(heartbeat, heartbeatInterval);
    };

    ws.onerror = function(error) {

    };

    ws.onmessage = function(event) {
        message = JSON.parse(event.data);

        if (message.error && (message.error == "ERR_BADAUTH")) {
        	//alert("OAuth Token is missing. Kindly contact the developer.");
        }

        if (message.type == 'RECONNECT') {
            setTimeout(connect, reconnectInterval);
        }



        if (message.data) {
	        var jsonmessage = JSON.parse(message.data.message);

	        if ((jsonmessage.type == "reward-redeemed") && jsonmessage.data.redemption) {
	        	var redeem = jsonmessage.data.redemption;
                redeemer = redeem.user.display_name;
	        	if (redeem.reward.title == redeemTitle)
	        		addToQueue("wish");
	        }
        }

    };

    ws.onclose = function() {
        clearInterval(heartbeatHandle);
        setTimeout(connect, reconnectInterval);
    };

}







function authUrl() {
	var clientId = 'wjwf4wolayi04w61r9jfj242z2j5v8';
	var redirectURI = 'https://sidestreamnetwork.net/GenshinTwitchRedeems';
    sessionStorage.twitchOAuthState = nonce(15);
    var url = 'https://id.twitch.tv/oauth2/authorize' +
        '?response_type=code' +
        '&client_id=' + clientId + 
        '&redirect_uri=' + redirectURI +
        '&state=' + sessionStorage.twitchOAuthState +
        '&scope=' + scope;
    return url
}



async function checkToken(localToken) {
	try {
		var url = 'https://id.twitch.tv/oauth2/validate';
		fetch(url, {
			headers: {Authorization: `Bearer ${localToken}`}
		})
		.then(resp => resp.json())
		.then(json => {
			return json;
		})		
	} catch (error) {
	}
	return false;
}



errorMessage = 'Twitch token is missing or invalid. Please use the Genshin Wisher app to get one.';

if (localToken != "") {
	user = checkToken(localToken);
	if (user !== false) {
		errorMessage = '';
	    twitchOAuthToken = localToken;
	    topics = ['channel-points-channel-v1.' + channelID];	// channelID should be in the "local_creds.js" file
	    connect(topics);
	}
}

if (errorMessage != '') {
    htmlContent = "<style>" +
        "#container { display: flex; height: 100vh; justify-content: center; align-items: center; font-family: sans-serif; }" +
        "#contents { display: flex; flex-wrap: wrap; gap: 0; min-width: 400px; }" +
        "#link_to_token {" +
            "font-weight: bold; font-size: 1.2rem; text-align: center; display: block; padding: 1em; margin: 0 auto;" +
            "background: #59f; color: #fff; text-decoration: none; width: 50%; text-shadow: 2px 2px 2px rgb(0 0 0 / 30%); border-radius: 15px;" +
        "}" +
        "h1, .elements { flex-basis: 100%; text-align: center; }" +
        "h1 { margin: 0; color: #666; } " +
        ".error { background: #f95; }" +
        "</style>";
    htmlContent += "<div id=\"container\">" +
        "<div id=\"contents\">" +
        "<h1>Genshin Impact: Wish On Stream</h1>" +
        "<div id=\"link_to_token\">" + errorMessage + "</div>" +
        "</div>" +
        "</div>";
    document.getElementById('wrapper').innerHTML = htmlContent;	
}
