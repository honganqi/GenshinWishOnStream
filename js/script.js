/* Genshin Impact: Twitch Redeem Wisher v1.0 by honganqi */
/* https://github.com/honganqi/GenshinImpact-TwitchRedeemWish */

var ws;

var queue = [];
var queueItems = [];

var isRunning = false;
var lastItemRun = 0;

var redeemer;


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
	setTimeout(function () {
		audio.play();
	}, 500);
	
	let star = randomInt(cases);
	let choice = Math.floor(Math.random() * (choices[star].length));
	let character = choices[star][choice];
	let element = 'img/elements/' + elementDictionary(character);
	let character_image_filename = `img/characters/${character}`;

	if (star <= 3)
		character_image_filename = 'img/characters/dull_blades/' + getDullBlades();

	var extensions = ['webp', 'png', 'svg'];
	extensions.forEach((ext) => {
		checker: if (imageExists(character_image_filename + '.' + ext)) {
			character_image_filename = character_image_filename + '.' + ext;
			break checker;
		}
	});

	extensions.forEach((ext) => {
		checker: if (imageExists(element + '.' + ext)) {
			element = element + '.' + ext;
			break checker;
		}
	});


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

function imageExists(image_url) {

    var http = new XMLHttpRequest();

    http.open('HEAD', image_url, false);
    http.send();

    return http.status != 404;
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
        if (message.error && (message.error == "ERR_BADAUTH"))
        	alert("OAuth Token is missing. Kindly contact the developer.");

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

function getChannelInfo(url) {
    $.ajax({
        beforeSend: function(request) {
            request.setRequestHeader("Client-Id", clientId);
            request.setRequestHeader("Authorization", "Bearer " + twitchOAuthToken);
        },
        dataType: "json",
        url: "https://api.twitch.tv/helix" + url
    })
    .done(function(data) {
        var channel_id = data.data[0].id;
        topics = ['channel-points-channel-v1.' + channel_id];
        connect(topics);
    });
}


getChannelInfo("/users?login=" + channelName);