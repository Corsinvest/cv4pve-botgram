// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';
import corsinvestTheme from '@corsinvest/cv4pve-docs-theme';

export default defineConfig({
  site: 'https://corsinvest.github.io',
  base: '/cv4pve-botgram',
  integrations: [
    starlight({
      title: 'cv4pve-botgram',
      description: 'Telegram bot to manage and monitor a Proxmox VE cluster from a phone.',
      // Brand, logo, GitHub and "Edit page" links, the Corsinvest sidebar group and
      // external links in a new tab come from the shared cv4pve theme.
      plugins: [
        corsinvestTheme({
          repo: 'cv4pve-botgram',
          // Product icon: favicon and header, dark variant for the dark theme.
          icon: { light: '/icon.svg', dark: '/icon-dark.svg' },
          // Banner on the home page: the same engine runs inside cv4pve-admin.
          admin: { module: 'bots' },
          // Install-and-run panel in the home hero.
          install: {
            targets: ['linux', 'windows'],
            run: ['--host=pve01', "--api-token='bot@pve!bot=…'", "--token='<telegram-bot-token>'", '--chatsId=<chat-id>'],
            output: [{ text: 'Start listening', tone: 'ok' }],
          },
        }),
      ],
      lastUpdated: true,
      sidebar: [
        {
          label: 'Start here',
          items: ['getting-started', 'telegram-bot', 'permissions', 'connection', 'service', 'troubleshooting'],
        },
        {
          label: 'Commands',
          items: [
            { label: 'Overview', slug: 'commands' },
            'commands/api',
            'commands/aliases',
          ],
        },
      ],
    }),
  ],
});
