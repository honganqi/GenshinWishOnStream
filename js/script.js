/* Genshin Impact: Twitch Redeem Wisher v1.1 by honganqi */
/* https://github.com/honganqi/GenshinImpact-TwitchRedeemWisher */

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
	cases[previousRate + rate] = star;
	previousRate += rate;
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



function genshinWish() {
	let audio = new Audio("sounds/character_appearance.ogg");

	let star = randomInt(cases);
	let choice = Math.floor(Math.random() * (choices[star].length));
	let character = choices[star][choice];
	element = 'img/elements/' + elementDictionary(character);
	character_image_filename = `img/characters/${character}`;

	if (star <= 3)
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
		<h1 id="name">${character}</h1>
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
    	console.log("success: " + image_url);
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
    	console.log("error: " + image_url);
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
	xmlhttp.send(`name=${user}&character=${character}`);
}












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














function getChannelID(channelName) {
	var sendState = sessionStorage.twitchOAuthState;
    return $.ajax({
        url: "https://sidestreamnetwork.net/GenshinTwitchRedeems/genshinWisher.php",
        dataType: "json",
        data: {type: "info", channelName: channelName, state: sendState}        
    });
}



getChannelID(channelName).done(function(data) {
	if (data.error !== undefined) {
		console.log("Twitch OAuth Token not found or expired. Redirecting to Twitch for authorization...");
		window.location.href = twitchAuthURL;
		//document.getElementById('wrapper').innerHTML = '<a id="link_to_token" href="' + twitchAuthURL + '">Twitch OAuth Token is not yet set. Please click here to authorize the Genshin Wisher to read your channel point redemptions on Twitch.</a>';
	}
    var channel_id = data.id;
    twitchOAuthToken = data.token;
    topics = ['channel-points-channel-v1.' + channel_id];
    connect(topics);
});