import { createRouter, createWebHistory } from 'vue-router'
import ReaderView from './views/ReaderView.vue'
import BookmarksView from './views/BookmarksView.vue'
import ChurchesView from './views/ChurchesView.vue'
import PathView from './views/PathView.vue'
import ResourcesView from './views/ResourcesView.vue'
import SystemSettingsView from './views/SystemSettingsView.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', name: 'reader', component: ReaderView },
    { path: '/bookmarks', name: 'bookmarks', component: BookmarksView },
    { path: '/churches', name: 'churches', component: ChurchesView },
    { path: '/path', name: 'path', component: PathView },
    { path: '/resources', name: 'resources', component: ResourcesView },
    { path: '/settings', name: 'settings', component: SystemSettingsView },
    { path: '/about', redirect: '/path' },
    { path: '/logs', redirect: '/settings' },
  ],
})

export default router
