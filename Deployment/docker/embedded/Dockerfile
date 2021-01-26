FROM node AS build

WORKDIR /app
COPY ./ChromeExt ./

RUN npm install
RUN npm run build


FROM nginx:1.19.0-alpine

RUN apk update && apk add certbot

WORKDIR /app
RUN sed -i "s|#gzip|gzip_types text/css application/javascript;\ngzip|g" /etc/nginx/nginx.conf
COPY ./Deployment/certbot/run.sh /etc/periodic/daily/run.sh
RUN chmod +x /etc/periodic/daily/run.sh
COPY ./Deployment/html/* /usr/share/nginx/html
COPY --from=build /app/dist /usr/share/nginx/html/v1
