import { createSwaggerSpec } from 'next-swagger-doc';

export const getApiDocs = async () => {
  const spec = createSwaggerSpec({
    apiFolder: 'src/app/api',
    definition: {
      openapi: '3.0.0',
      info: {
        title: 'Power4You API Documentation',
        version: '1.0.0',
        description: 'API endpoints for Power4You platform',
        contact: {
          name: 'Power4You Support',
          email: 'power4you@gbs-labor.de',
        },
      },
      components: {
        securitySchemes: {
          cookieAuth: {
            type: 'apiKey',
            in: 'cookie',
            name: 'auth-token',
          },
          apiKey: {
            type: 'apiKey',
            in: 'header',
            name: 'x-api-key',
            description: 'API key from user profile to authenticate API calls',
          },
        },
      },
      security: [
        {
          cookieAuth: [],
        },
        {
          apiKey: [],
        },
      ],
    },
  });
  return spec;
}; 