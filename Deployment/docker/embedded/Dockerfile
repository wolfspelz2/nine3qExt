# === build js
FROM node AS build

WORKDIR /app
COPY ./ChromeExt ./

RUN npm install
RUN npm run release-embedded

# === setup webserver
FROM nginx:1.19.0-alpine

RUN apk update && apk add certbot

WORKDIR /app
RUN sed -i "s|#gzip|gzip_types text/css application/javascript;\ngzip|g" /etc/nginx/nginx.conf

COPY ./Deployment/certbot/run.sh /etc/periodic/daily/run.sh
RUN chmod +x /etc/periodic/daily/run.sh
COPY ./Deployment/html /usr/share/nginx/html
COPY ./Deployment/docker/embedded/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html/v1
