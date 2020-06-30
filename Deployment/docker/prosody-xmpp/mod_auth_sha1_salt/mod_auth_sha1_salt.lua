-- Copyright (C) 2018 Minddistrict
--
-- This file is MIT/X11 licensed.
--

local host = module.host;
local log = module._log;
local new_sasl = require "util.sasl".new;
local usermanager = require "core.usermanager";
local sha1 = require "util.hashes".sha1;
local provider = {};


function provider.test_password(username, password)
    local sv = sha1('3b6f88f2bed0f392' .. username, true);
	log("info", "Testing password for user %s at host %s (should be %s)", username, host, sv);
    if sv == password then
		return true;
	else
		return nil, "Auth failed. Invalid username or password.";
	end
	return true;
end

function provider.users()
	return function()
		return nil;
	end
end

function provider.set_password(username, password)
	return nil, "Changing passwords not supported";
end

function provider.user_exists(username)
	return true;
end

function provider.create_user(username, password)
	return nil, "User creation not supported";
end

function provider.delete_user(username)
	return nil , "User deletion not supported";
end

function provider.get_sasl_handler()
	return new_sasl(host, {
		plain_test = function(sasl, username, password, realm)
			return usermanager.test_password(username, realm, password), true;
		end
	});
end

module:provides("auth", provider);
