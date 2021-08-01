
import { predictFromFileContents } from "./prediction";

import * as http from 'http'
import * as httpProxy from 'http-proxy'

// const proxy = httpProxy.createProxyServer({});
// var option = {
//   target: target,
//   selfHandleResponse : true
// };
// httpProxy.on('proxyRes', function (proxyRes, req, res) {
//     var body = [];
//     proxyRes.on('data', function (chunk) {
//         body.push(chunk);
//     });
//     proxyRes.on('end', function () {
//         body = Buffer.concat(body).toString();
//         console.log("res from proxied server:", body);
//         res.end("my response to cli");
//     });
// });
// httpProxy.web(req, res, option);
