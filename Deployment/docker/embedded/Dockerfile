FROM node AS build

WORKDIR /app
COPY ./ChromeExt ./

RUN npm install
RUN npm run build:embedded


FROM nginx:1.19.0-alpine
WORKDIR /app
COPY --from=build /app/dist /usr/share/nginx/html/v1
