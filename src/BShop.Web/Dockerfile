FROM node:10-alpine as build
WORKDIR /app

COPY package*.json .
COPY yarn.lock .
RUN yarn

COPY . .
RUN yarn build

FROM nginx:1.16.0-alpine
COPY --from=build /app/build /usr/share/nginx/html
EXPOSE 80

ENTRYPOINT ["nginx", "-g", "daemon off;"]
