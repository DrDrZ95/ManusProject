/**
 * Project Service - 项目管理服务
 * 
 * 作用:
 * 管理项目列表、部署状态、Git 历史记录等业务逻辑。
 */

import { http } from '../utils/http';
import { ProjectStatus } from '../types';
import { MOCK_PROJECTS } from '../constants';

const USE_MOCK = true;

class ProjectService {

  /**
   * 获取所有项目状态
   * GET /api/v1/projects
   */
  public async getProjects(): Promise<ProjectStatus[]> {
    if (USE_MOCK) {
      return new Promise(resolve => setTimeout(() => resolve(MOCK_PROJECTS), 400));
    }
    return http.get<ProjectStatus[]>('/projects');
  }

  /**
   * 创建新项目
   * POST /api/v1/projects
   */
  public async createProject(data: Partial<ProjectStatus>): Promise<ProjectStatus> {
    if (USE_MOCK) {
        return new Promise(resolve => {
            setTimeout(() => {
                const newProject = {
                    ...data,
                    id: Date.now().toString(),
                    status: 'healthy',
                    uptime: '0%',
                    lastDeployment: 'Never',
                    branch: 'main'
                } as ProjectStatus;
                resolve(newProject);
            }, 2000);
        });
    }
    return http.post<ProjectStatus>('/projects', data);
  }

  /**
   * 触发项目部署
   * POST /api/v1/projects/{id}/deploy
   */
  public async deployProject(id: string, branch: string = 'main'): Promise<{ deploymentId: string }> {
      if (USE_MOCK) {
          return new Promise(resolve => setTimeout(() => resolve({ deploymentId: `dep-${Date.now()}` }), 1500));
      }
      return http.post(`/projects/${id}/deploy`, { branch });
  }
}

export const projectService = new ProjectService();