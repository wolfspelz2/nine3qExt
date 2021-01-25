FROM node AS build

WORKDIR /app
COPY ./ChromeExt ./

RUN npm install
RUN npm run build:embedded

#ed -s /etc/ssh/sshd_config <<< $'/Port 22/s/22/57831/g\nw'

FROM nginx:1.19.0-alpine
WORKDIR /app
RUN sed -i "s|#gzip|gzip_types text/css application/javascript;\ngzip|g" /etc/nginx/nginx.conf
COPY --from=build /app/dist /usr/share/nginx/html/v1
