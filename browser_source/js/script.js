/* Genshin Impact: Wish On Stream v1.8 by honganqi */
/* https://github.com/honganqi/GenshinWishOnStream */

var wrapper = document.getElementById('wrapper');

var queue = [];
var queueItems = [];

var isRunning = false;
var lastItemRun = 0;

var redeemer;

let character_image_filename;
let element;

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
	Promise.all(loaders).then(function() {
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
	return new Promise((resolve) => {
		const img = new Image();
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
			resolve();
		};
		img.onerror = () => {
			resolve();
		};
	});
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

function displayTokenExpired(errorMessage) {
    var htmlContent = "<style>" +
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
    return htmlContent;
}





class TwitchMonitor {
	heartbeatInterval = 1000 * 60; //ms between PING's
	reconnectInterval = 1000 * 3; //ms to wait before reconnect
	heartbeatHandle;
	scope = '';
	ws;
	twitchOAuthToken = '';
	topics = '';
	message = {};
	
	constructor({ type, token, scope, topics = '' }) {
		this.scope = encodeURIComponent(scope);
		this.twitchOAuthToken = token;
		this.topics = topics;
		this.ws = new WebSocket('wss://irc-ws.chat.twitch.tv');
	}

	// these 5 functions below are thanks to Twitch Developers over at GitHub
	// https://github.com/twitchdev/pubsub-javascript-sample

	// Source: https://www.thepolyglotdeveloper.com/2015/03/create-a-random-nonce-string-using-javascript/
	nonce(length) {
		var text = "";
		var possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
		for (var i = 0; i < length; i++) {
			text += possible.charAt(Math.floor(Math.random() * possible.length));
		}
		return text;
	}

	heartbeat() {
		this.message = {
			type: 'PING'
		};
		this.ws.send(JSON.stringify(this.message));
	}

	listen() {
		this.message = {
			type: 'LISTEN',
			nonce: this.nonce(15),
			data: {
				topics: this.topics,
				auth_token: this.twitchOAuthToken
			}
		};
		this.ws.send(JSON.stringify(this.message));
	}

	chat() {
		const cs = this.ws;
        
		cs.onopen = () => {
			cs.send('PASS oauth:' + this.twitchOAuthToken + '\r\n');
			cs.send('NICK piknik_rules\r\n');
			cs.send('CAP REQ :twitch.tv/commands twitch.tv/tags\r\n');

			this.heartbeat();
			this.heartbeatHandle = setInterval(this.heartbeat.bind(this), this.heartbeatInterval);
		};
	
		cs.onerror = function(error) {
	
		};
	
		cs.onmessage = function(event) {
			event.data.split('\r\n').forEach(async line => {
				if (!line) return;
				var message = parseIRC(line);
				if (!message.command) return;

				switch (message.command) {
					case "PING":
						cs.send('PONG ' + message.params[0]);
						return;
					case "001":
						cs.send('JOIN #' + channelName + '\r\n');
						return;
					case "JOIN":
						console.log('Genshin Wisher: ready to accept Twitch commands (' + twitchCommandPrefix + ')');
						return;
					case "CLEARMSG":
					case "CLEARCHAT":
						return;
					case "PRIVMSG":
						if (message.params[0] !== '#' + channelName || !message.params[1] || !message.params[1].toLowerCase().startsWith(twitchCommandPrefix)) return;
						redeemer = message.params[0].replace('#', '');
						addToQueue("wish");
						return;
					case "NOTICE":
						// need to test expired tokens
						if (message.params && message.params[1]) {
							switch (message.params[1]) {
								case "Login authentication failed":
									cs.send('PART #' + channelName + '\r\n')
									clearInterval(this.heartbeatHandle);
									var errorMessage = 'Twitch token is missing or invalid. Please use the Genshin Wisher app to get one.';
									wrapper.innerHTML = displayTokenExpired(errorMessage);
							}
						}
						return;
				}
			});
		};
	
		cs.onclose = function() {

		};
    }    
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
		console.log('checkToken error:', error);
		return false;
	}
}



errorMessage = 'Twitch token is missing or invalid. Please use the Genshin Wisher app to get one.';

if (localToken != "") {
	checkToken(localToken)
	.then(user => {
		if (user !== false) {
			errorMessage = '';

			if (twitchCommandEnabled && twitchCommandPrefix != '') {
				const twitchChat = new TwitchMonitor({
					type: 'chat',
					token: localToken,
					scope: 'chat:read'
				});
				twitchChat.chat();
			}
		} else {
			if (errorMessage != '') {
				wrapper.innerHTML = displayTokenExpired(errorMessage);
			}
		}
	})
}


const backendUrl = 'https://genshin-twitch.sidestreamnetwork.net';

// Subscribe to redemptions
async function subscribe() {
	const genshinWisherClientId = 'rs83ihxx7l4k7jjeprsiz03ofvly8g';
	const res = await fetch(`${backendUrl}/subscribe`, {
		method: 'POST',
		headers: {
			'Content-Type': 'application/json',
			'Client-Id': genshinWisherClientId
		},
		body: JSON.stringify({
			broadcaster_user_id: channelID,
			access_token: localToken
		})
	})
	.then(res => res.json())
	.then(data => {
		console.log('Subscribed, waiting for wishes:', data);
	})
	.catch(err => console.error("Fetch error:", err));        
}

async function listen() {
	const eventSource = new EventSource(`${backendUrl}/event-stream?broadcaster_user_id=${channelID}`);

	eventSource.onmessage = (event) => {
		const redemption = JSON.parse(event.data);
		if (redemption.reward.title === redeemTitle) {
			redeemer = redemption.user_name;
			addToQueue("wish");
		}
	};

	eventSource.onerror = (err) => {
		console.error('Error on listen() - Event stream error:', err);
		eventSource.close();
	};
}

// Initialize
subscribe();
listen();