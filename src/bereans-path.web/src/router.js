import { createRouter, createWebHistory } from 'vue-router'
import ReaderView from './views/ReaderView.vue'
import BookmarksView from './views/BookmarksView.vue'
import ChurchesView from './views/ChurchesView.vue'
import ResourcesView from './views/ResourcesView.vue'
import SystemSettingsView from './views/SystemSettingsView.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', name: 'reader', component: ReaderView },
    { path: '/bookmarks', name: 'bookmarks', component: BookmarksView },
    { path: '/churches', name: 'churches', component: ChurchesView },
    { path: '/resources', name: 'resources', component: ResourcesView },
    { path: '/settings', name: 'settings', component: SystemSettingsView },
    { path: '/logs', redirect: '/settings' },
  ],
})

export default router
