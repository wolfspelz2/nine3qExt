FROM prosody/prosody
RUN chmod 755 /etc/prosody/certs/localhost.key
COPY ./conf.d /etc/prosody/conf.d
COPY ./mod_auth_sha1_salt /usr/lib/prosody/modules
RUN echo 'Include "conf.d/*.cfg.lua"' >> /etc/prosody/prosody.cfg.lua


