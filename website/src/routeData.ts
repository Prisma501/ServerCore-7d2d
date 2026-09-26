import { defineRouteMiddleware } from '@astrojs/starlight/route-data';

// src/content/docs is a symlink to the top-level docs/ folder, and GitHub can't edit through a symlink.
export const onRequest = defineRouteMiddleware((context) => {
	const { editUrl } = context.locals.starlightRoute;
	if (editUrl) editUrl.pathname = editUrl.pathname.replace('/website/src/content/docs/', '/docs/');
});
