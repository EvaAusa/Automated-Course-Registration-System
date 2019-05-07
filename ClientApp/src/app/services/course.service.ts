import { Injectable } from '@angular/core';
import { JwtHelperService } from '@auth0/angular-jwt';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthenticationService } from './authentication.service';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Course } from '../models/course';
const endpoint = environment.apiEndpoint + 'courses/';
@Injectable({
  providedIn: 'root'
})
export class CourseService {
  private jwtHelper: JwtHelperService;


  /**
   * Constructor for Grade Service
   * @param http 
   * @param authService 
   * @param router 
   */
  constructor(
    private http: HttpClient,
    private authService: AuthenticationService,
    private router: Router

  ) {
    this.jwtHelper = new JwtHelperService();
    if (this.jwtHelper.isTokenExpired(localStorage.getItem('currentUser'))) {
      this.authService.logout();
      this.router.navigate(['login', { expired: true }]);
    }
  }

  /**
   * Http get, return a list of all courses
   */
  public getCourses(): Observable<any> {
    return this.http.get<any>(endpoint, this.getHttpHeaders())
      .pipe(map((response: Response) => response || {}));
  }

  public getCourse(course: Course) {
    let id = course.courseId;
    this.http.get<any>(endpoint + id, this.getHttpHeaders())
      .subscribe();
    return course;
  }
  public deleteCourse(course: Course) {
    let id = course.courseId;
    this.http.delete<any>(endpoint + id, this.getHttpHeaders())
      .subscribe();
    return course;
  }

  public addCourse(course: Course) {

    return this.http.post<any>(endpoint, course, this.getHttpHeaders())
      .pipe(map((response: Response) => response || {}));

  }



  // HTTP headers
  private getHttpHeaders(): {} {
    return {
      headers: new HttpHeaders({
        'Authorization': 'Bearer ' + localStorage.getItem('token'),
        'Content-Type': 'application/json'
      })
    };
  }
}