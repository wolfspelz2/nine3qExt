FROM node AS build

WORKDIR /app
COPY ./ChromeExt ./

RUN npm install
RUN npm run build:embedded

#ed -s /etc/ssh/sshd_config <<< $'/Port 22/s/22/57831/g\nw'

FROM nginx:1.19.0-alpine
WORKDIR /app
RUN ed /etc/nginx/nginx.conf <<< $'/#gzip/s/#//g\nw'
COPY --from=build /app/dist /usr/share/nginx/html/v1
