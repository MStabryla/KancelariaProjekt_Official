import * as moment from "moment";
import { isMoment } from "moment";

export function AssignToModel<T>(to: T, from: any) {
  Object.entries(to).forEach(([k, v]) => {
    if (from && from.hasOwnProperty(k)) {
      if (Array.isArray(from[k])) {
        const newElement = { ...v[0] };
        delete v[0];
        Object.entries(from[k]).forEach(([ak, av]) => {
          v[ak] = { ...newElement };
          AssignToModel(v[ak], av);
        });
      } else {
        if (from[k] instanceof Object) {
          if (isMoment(from[k])) {
            to[k] = from[k].format('YYYY-MM-DDTHH:mm:ss');
          } else {
            to[k] = { ...to[k] };
            AssignToModel(to[k], from[k]);
          }
        } else {
          to[k] = from[k];
        }
      }
    } else {
      delete to[k];
    }
  });
};
