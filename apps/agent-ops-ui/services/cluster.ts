/**
 * Cluster Service - Kubernetes 集群数据服务
 * 
 * 设计模式: Service Layer / Adapter Pattern
 * 作用:
 * 封装所有与 K8s 集群相关的 API 调用。
 * 允许在 Mock 数据和真实 API 之间无缝切换，UI 组件无需感知数据来源。
 */

import { http } from '../utils/http';
import { ClusterNode, K8sPod } from '../types';
import { MOCK_CLUSTER_NODES, MOCK_PODS } from '../constants';

// 模拟开关：在开发阶段使用 Mock 数据
const USE_MOCK = true;

class ClusterService {
  
  /**
   * 获取集群节点列表
   * GET /api/v1/nodes
   */
  public async getNodes(): Promise<ClusterNode[]> {
    if (USE_MOCK) {
      return new Promise(resolve => setTimeout(() => resolve(MOCK_CLUSTER_NODES), 500));
    }
    return http.get<ClusterNode[]>('/nodes');
  }

  /**
   * 获取 Pod 列表
   * GET /api/v1/pods?namespace={namespace}
   * @param namespace 可选的命名空间过滤
   */
  public async getPods(namespace?: string): Promise<K8sPod[]> {
    if (USE_MOCK) {
      return new Promise(resolve => {
        setTimeout(() => {
          const pods = namespace 
            ? MOCK_PODS.filter(p => p.namespace === namespace)
            : MOCK_PODS;
          resolve(pods);
        }, 600);
      });
    }
    return http.get<K8sPod[]>('/pods', { params: { namespace } });
  }

  /**
   * 获取特定 Pod 的详细信息 (包含 eBPF 指标)
   * GET /api/v1/pods/{id}
   */
  public async getPodDetails(id: string): Promise<K8sPod | undefined> {
    if (USE_MOCK) {
        return new Promise(resolve => {
            const pod = MOCK_PODS.find(p => p.id === id);
            setTimeout(() => resolve(pod), 300);
        });
    }
    return http.get<K8sPod>(`/pods/${id}`);
  }

  /**
   * 重启某个 Pod
   * POST /api/v1/pods/{id}/restart
   */
  public async restartPod(id: string): Promise<boolean> {
      console.log(`[ClusterService] Requesting restart for pod ${id}`);
      if (USE_MOCK) {
          return new Promise(resolve => setTimeout(() => resolve(true), 1000));
      }
      return http.post(`/pods/${id}/restart`);
  }
}

export const clusterService = new ClusterService();