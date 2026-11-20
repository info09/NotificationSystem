FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

COPY src/AdminUI/src/index.html ./index.html
COPY src/AdminUI/src ./src

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
